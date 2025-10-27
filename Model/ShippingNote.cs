namespace WMS.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ShippingNote
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ShippingNote()
        {
            ShippingNoteDetails = new HashSet<ShippingNoteDetail>();
        }

        [Key]
        public int NoteID { get; set; }

        public DateTime NoteDate { get; set; }

        public int CustomerID { get; set; }

        public int WarehouseID { get; set; }

        public int? EmployeeID { get; set; }

        public decimal? TotalAmount { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Employee Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShippingNoteDetail> ShippingNoteDetails { get; set; }
    }
}
