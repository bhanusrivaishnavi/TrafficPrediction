using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCapplication.Models
{

    //public class FileUpload File {get;set;}

    public class FileUpload
    {
        [Key]
        [Column(TypeName = "bigint")]
        public long F_ID { get; set; }
        //adding foregn key of user id
        
        [Column(TypeName = "varchar(max)")]
        public string UserName { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string FilePath { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string FileName { get; set; }



        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string IsProcessed { get; set; }

      
        [Column(TypeName = "varchar(max)")]
        public string FullName { get; set; }

    }
}


