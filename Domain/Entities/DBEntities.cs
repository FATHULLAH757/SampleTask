using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    
    public class UploadFile {
        [Key]
        public int FileId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public float Size { get; set; }
        public string FilePath { get; set; }
    }
}
