namespace WMS.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ShippingNoteDetail
    {
        [Key]
        public int DetailID { get; set; }

        public int NoteID { get; set; }

        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? LineTotal { get; set; }

        public virtual ShippingNote ShippingNote { get; set; }

        // Navigation to product (added)
        public virtual Product Product { get; set; }
    }
}
