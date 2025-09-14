using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class LogCategory : IEntity<int>
    {
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public string ControllerFaName { get; set; }
        public string ActionName { get; set; }
        public string ActionFaName { get; set; }
    }
}
