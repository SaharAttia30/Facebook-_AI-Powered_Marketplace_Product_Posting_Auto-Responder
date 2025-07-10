using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBlaster.Models
{
    public class Group
    {
        [Key]                                 // PK
        public string GroupId { get; set; }  // facebook numeric id or Guid
        public string AvatarId { get; set; }  // FK → Avatar
        public string Name { get; set; } // group title
        public string Url { get; set; } // https://www.facebook.com/groups/…

        public string City { get; set; }
        public string State { get; set; }

        [ForeignKey(nameof(AvatarId))]
        public virtual Avatar Avatar { get; set; }
        public Group()
        {

        }
    }
    
}
