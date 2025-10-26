namespace WMS.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Supplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Supplier()
        {
            Products = new HashSet<Product>();
            ReceivingNotes = new HashSet<ReceivingNote>();
        }

        public int SupplierID { get; set; }

        [Required]
        [StringLength(250)]
        public string SupplierName { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; }

        [StringLength(10)]
        public string Phone { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReceivingNote> ReceivingNotes { get; set; }
    }
}
