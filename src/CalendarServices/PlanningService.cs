using CalendarServices.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CalendarServices
{
    public class PlanningService : IPlanningService
    {
        ILogger Logger;

        public PlanningService(ILogger? logger)
        {
            this.Logger = logger; 
        }

        /// <summary>
        /// Maps a model to an Xml.
        /// Returns the xml, or throws if a model is not serializable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Xml String></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> ObjectToXml(object obj) =>
            obj switch
            {
                PlanningAttendee a => await ToXml((PlanningAttendee)obj),
                PlanningSession e => await ToXml((PlanningSession)obj),
                PlanningSessionAttendee e => await ToXml((PlanningSessionAttendee)obj),
                null => throw new ArgumentNullException($"Object to serialize cannot be null."),
                _ => throw new ArgumentException($"Cannot serialize object of type {obj.GetType()} to XML.")
            };
        

        private async Task<string> ToXml<T>(T obj)
        {
            var returnString = "";
            
            // convert object to stream
            using var stream = new MemoryStream();

            //Directory.CreateDirectory("C:\\temp\\");
            //FileStream fs = new FileStream(@"C:\temp\brol.xml", FileMode.Create);
            //serializer.Serialize(fs, obj);
            //fs.Close();

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stream, obj);

                stream.Position = 0;
                var reader = new StreamReader(stream);
                var stringske = await reader.ReadToEndAsync();
                
                var document = new XmlDocument();
                document.LoadXml(stringske);

                #region xsd for validation
                var xmlSchemaSet = new XmlSchemaSet();
                
                //string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string basePath = @"..\..\..\..\src\CalendarServices";
                
                xmlSchemaSet.Add("", basePath + @"\XmlSchemas\AttendeeEvent.xsd");
                xmlSchemaSet.Add("", basePath + @"\XmlSchemas\SessionAttendeeEvent.xsd");
                xmlSchemaSet.Add("", basePath + @"\XmlSchemas\SessionEvent.xsd");
                xmlSchemaSet.Add("", basePath + @"\XmlSchemas\UUID.xsd");
                #endregion
                
                document.Schemas.Add(xmlSchemaSet);
                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                // Will throw if validation fails.
                document.Validate(eventHandler);

                returnString = stringske;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while retrieving message from queue.");
                return $"Error: {ex.Message}";
            }

            return returnString;

        }
        
        private void ValidationEventHandler(object? sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    throw new InvalidDataException($"XML validation failed. Error: {e.Message}");
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    throw new InvalidDataException($"XML validation failed. Error: {e.Message}");
            }
        }
    }
}
