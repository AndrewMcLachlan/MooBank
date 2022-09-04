using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Asm.MooBank.Data.Entities
{
    public partial class ImportAccount
    {
       // [ForeignKey("ImportAccount")]
        public Guid AccountId { get; set; }

        public int ImporterTypeId { get; set; }

        public virtual ImporterType ImporterType { get; set; }

        public virtual Account Account { get; set; }
    }
}
