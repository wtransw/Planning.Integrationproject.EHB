using CalendarServices;
using CalendarServices.Models;
using CalendarServices.Models.Configuration;
using Crm.Link.RabbitMq.Producer;
using Crm.Link.UUID;

namespace PlanningApi.Workers;

public class PollingScheduler : IHostedService
{
    private readonly ILogger<PollingScheduler> Logger;
    private readonly IUUIDGateAway UuidMaster;
    private readonly IGoogleCalendarService GoogleCalendarService;
    private readonly PlanningAttendeePublisher PlanningAttendeePublisher;
    private readonly PlanningSessionPublisher PlanningSessionPublisher;
    private readonly PlanningSessionAttendeePublisher PlanningSessionAttendeePublisher;
    private List<string> ObjectsFound = new List<string>();

    public PollingScheduler(
        ILogger<PollingScheduler> logger,
        IUUIDGateAway uuidMaster,
        IGoogleCalendarService googleCalendarService,
        ICalendarOptions calendarOptions,
        PlanningAttendeePublisher planningAttendeePublisher,
        PlanningSessionPublisher planningSessionPublisher,
        PlanningSessionAttendeePublisher planningSessionAttendeePublisher
        )
    {
        this.Logger = logger;
        this.UuidMaster = uuidMaster;
        this.GoogleCalendarService = googleCalendarService;
        googleCalendarService.CreateCalendarService(calendarOptions);
        this.PlanningAttendeePublisher = planningAttendeePublisher;
        this.PlanningSessionPublisher = planningSessionPublisher;
        this.PlanningSessionAttendeePublisher = planningSessionAttendeePublisher;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Planning Polling Scheduler started.");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                //Poll Google Calendar for changes.
                var upcomingSessions = await GoogleCalendarService.GetAllUpcomingSessions(GoogleCalendarService.CalendarGuid);
                var upcomingSessionsNext30Days = upcomingSessions.Where(s => s.Start.DateTime < DateTime.Now.AddDays(30));

                foreach (var session in upcomingSessionsNext30Days)
                {
                    var sessionUuid = await UuidMaster.GetGuid(session.Id, SourceEnum.PLANNING.ToString(), Crm.Link.UUID.Model.EntityTypeEnum.Session);

                    //mogelijk gewijzigd.
                    if (sessionUuid != null)
                    {
                        var uuidResource = await UuidMaster.GetResource(sessionUuid.Uuid, SourceEnum.PLANNING.ToString());
                        var currentVersionNumber = uuidResource.EntityVersion;

                        //Kijken of er attendees bijgekomen zijn. Attendees die niet komen moeten gewoon hun invitationstatus veranderen. 

                        //foreach (var attendee in session.Attendees)
                        //{
                        //    var attendeeUuid = await UuidMaster.GetGuid(attendee.Email, SourceEnum.PLANNING.ToString(), Crm.Link.UUID.Model.EntityTypeEnum.Attendee);

                        //    //Nieuwe Attendee
                        //    if (attendeeUuid == null)
                        //    { 
                        //        string firstname = "John";
                        //        string lastname = "Doe";
                        //        string newAttendeeUuid = new Guid().ToString();
                        //        try
                        //        {
                        //            firstname = attendee.DisplayName.Split(' ')[0];
                        //            lastname = attendee.DisplayName.Split(' ')[1];
                        //            if (!string.IsNullOrEmpty(attendee.Id))
                        //                newAttendeeUuid = Guid.Parse(attendee.Id).ToString();
                        //        }
                        //        catch
                        //        {
                        //            Logger.LogError($"Error splitting name ");
                        //        }
                        //        var newAttendee = new PlanningAttendee()
                        //        {
                        //            Email = attendee.Email,
                        //            EntityVersion = 1,
                        //            Name = firstname,
                        //            LastName = lastname,
                        //            Method = MethodEnum.CREATE,
                        //            Source = SourceEnum.PLANNING,
                        //            SourceEntityId = attendee.Email,
                        //            UUID_Nr = newAttendeeUuid,
                        //            EntityType = "Attendee"
                        //        };

                        //        var newSessionAttendee = new PlanningSessionAttendee()
                        //        {
                        //            AttendeeUUID = Guid.NewGuid().ToString(),
                        //            EntityType = "SessionAttendee",
                        //            EntityVersion = 1,
                        //            InvitationStatus = attendee.ResponseStatus == "accepted" ? NotificationStatus.ACCEPTED
                        //                                : attendee.ResponseStatus == "declined" ? NotificationStatus.DECLINED
                        //                                : NotificationStatus.PENDING,
                        //            Method = MethodEnum.CREATE,
                        //            SessionUUID = sessionUuid.Uuid.ToString(),
                        //            Source = SourceEnum.PLANNING,
                        //            SourceEntityId = Guid.NewGuid().ToString(),
                        //            UUID_Nr = Guid.NewGuid().ToString()
                        //        };

                        //        PlanningAttendeePublisher.Publish(newAttendee);
                        //        PlanningSessionAttendeePublisher.Publish(newSessionAttendee);

                        //        session.Description = planningSession.UUID_Nr;
                        //        await GoogleCalendarService.UpdateSession(GoogleCalendarService.CalendarGuid, session);
                        //    }
                        //}

                    }
                    else if (!ObjectsFound.Contains(session.Description) && !ObjectsFound.Contains(session.Id))
                    {
                        //Nieuwe sessie aangemaakt via Calendar.
                        var sessionId = session.Id != null && session.Id.Length >= 32 ? session.Id : Guid.NewGuid().ToString();  
                        var organizerId = session.Organizer.Id != null && session.Organizer.Id.Length >= 32 ? session.Organizer.Id : Guid.NewGuid().ToString();
                        var planningSession = new PlanningSession()
                        {
                            Title = session.Summary,
                            StartDateUTC = session.Start.DateTime ?? DateTime.Parse(session.Start.DateTimeRaw),
                            EndDateUTC = session.End.DateTime ?? DateTime.Parse(session.End.DateTimeRaw),
                            EntityType = "SessionEvent",
                            IsActive = true,
                            EntityVersion = 1,
                            Method = MethodEnum.CREATE,
                            OrganiserUUID = organizerId,
                            SourceEntityId = sessionId,
                            Source = SourceEnum.PLANNING,
                            UUID_Nr = sessionId,
                        };
                        ObjectsFound.Add(sessionId);

                        PlanningSessionPublisher.Publish(planningSession);

                        //add the (new) Guid in comment
                        session.Description = planningSession.UUID_Nr;
                        await GoogleCalendarService.UpdateSession(GoogleCalendarService.CalendarGuid, session);

                        await UuidMaster.PublishEntity(Guid.Parse(planningSession.UUID_Nr), SourceEnum.PLANNING.ToString(), Crm.Link.UUID.Model.EntityTypeEnum.Session, organizerId, 1);
                    }
                }
                await Task.Delay(60 * 1000);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Gracefully shutting down Polling scheduler.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Polling scheduler error: {ex.Message}");
                await Task.Delay(2 * 60 * 1000);
            }
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
