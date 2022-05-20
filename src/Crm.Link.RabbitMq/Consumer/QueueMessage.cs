using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Link.RabbitMq.Consumer
{
    public class QueueMessage
    {
        public string JobType { get; set; }
        public Guid UUIKey { get; set; }
        public object DataResponse { get; set; }
    }
}
