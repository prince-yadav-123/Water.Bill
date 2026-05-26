using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PipeSizeMasterConfiguration : IEntityTypeConfiguration<PipeSizeMaster>
{
    public void Configure(EntityTypeBuilder<PipeSizeMaster> entity)
    {
        entity.HasKey(e => e.PipeSizeId).HasName("PRIMARY");
        
        entity.ToTable("pipe_size_master");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.PipeSize).HasColumnName("PIPE_SIZE");
        entity.Property(e => e.PipeSizeId)
            .ValueGeneratedOnAdd()
            .HasColumnName("PIPE_SIZE_ID");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
