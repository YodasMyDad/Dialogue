using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class FavouriteMapping : EntityTypeConfiguration<Favourite>
    {
        public FavouriteMapping()
        {
            ToTable("DialogueFavourite");
            HasKey(x => x.Id);
        }
    }
}