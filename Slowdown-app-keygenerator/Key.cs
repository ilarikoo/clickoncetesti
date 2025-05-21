using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Slowdown_app_keygenerator
{
    public class Key
    {
        public string CourseName { get; set; }
        public int CourseId { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))] 
        public DateOnly Created { get; set; }

        public string KeyValue { get; set; } 

        public Key(string courseName, string keyValue, DateOnly created)
        {
            CourseName = courseName;
            Created = created;
            KeyValue = keyValue;
        }

        // JsonIgnore siltä varalta ettei backendin puolella yritä deserialisoida näitä tietoja
        // Alla olevat tiedot ovat käytössä vain KeyGenin puolella
       
        [JsonIgnore]
        public DateOnly Expires => Created.AddMonths(6);

        [JsonIgnore]
        public bool IsValid => Expires >= DateOnly.FromDateTime(DateTime.Today);
    }
}
