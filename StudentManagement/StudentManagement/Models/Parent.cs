//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Parent
    {
        public Parent()
        {
            this.Students = new HashSet<Student>();
        }
    
        public System.Guid Id { get; set; }
        public string NameDad { get; set; }
        public string NameMom { get; set; }
        public string AddressDad { get; set; }
        public string AddressMom { get; set; }
        public string PhoneDad { get; set; }
        public string PhoneMom { get; set; }
        public string JobDad { get; set; }
        public string JobMom { get; set; }
    
        public virtual ICollection<Student> Students { get; set; }
    }
}
