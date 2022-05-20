using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Link.UUID.Model
{
    public class ResourceDto
    {
        public Guid Uuid { get; set; }
        public string Source { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }
    }
}
