using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data.Contexts;

public class Dsw2025TpiContext: DbContext
{
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       // modelBuilder.Ignore<Customer>();
        
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(eb => {

            eb.ToTable("Products");

            // eb.HasIndex(p => p._sku)//para que el indice de _sku sea unico
            //.IsUnique();
            eb.HasKey(p => p.Id);

            eb.Property(p => p.InternalCode)
            .HasMaxLength(50)
            .IsRequired();


            eb.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();


            eb.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();



            eb.Property(p => p.Description)
            .HasMaxLength(100);


            eb.Property(p => p.CurrentUnitPrice)
            .HasPrecision(15, 2);

        


        });



        modelBuilder.Entity<Customer>(eb => {
            eb.ToTable("Customers");
            eb.HasKey(p => p.Id);

            eb.Property(c => c.UserName)
                .HasMaxLength(450) // Tamaño estándar de Identity
                .IsRequired();


            eb.Property(c => c.Name)
               .HasMaxLength(100)
               .IsRequired();

            eb.Property(c => c.Email)
                .HasMaxLength(255)
                .IsRequired();

            eb.Property(c => c.PhoneNumber)
                .HasMaxLength(20);

            // Índice único para email
            eb.HasIndex(c => c.Email)
                .IsUnique();

        });
        modelBuilder.Entity<Order>(eb => {
            eb.ToTable("Orders");
            eb.HasKey(p => p.Id);

            eb.HasOne<Customer>()// relacion muchos a uno
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

            eb.Property(o => o.ShippingAddress)
               .HasMaxLength(500)
               .IsRequired();

            eb.Property(o => o.BillingAddress)
                .HasMaxLength(500)
                .IsRequired();

            eb.Property(o => o.Notes)
                .HasMaxLength(1000);

            eb.Ignore(o => o.TotalAmount); 

            eb.Property(o => o.Status)
                .HasConversion<string>() // Guarda el enum como string
                .HasMaxLength(50);



        });
        modelBuilder.Entity<OrderItem>(eb => {
            eb.ToTable("OrderItems");
            eb.HasKey(p => p.Id);

            // Solo si Order.Id y OrderItem.OrderId son del mismo tipo
            /* eb.HasOne<Order>()
                 .WithMany()
                 .HasForeignKey(oi => oi.OrderId)
                 .HasPrincipalKey(o => o.Id);  // Especificar la clave principal
            */
            eb.HasOne<Order>()
         .WithMany(o => o.OrderItems)  // Especificar la lista explícitamente
         .HasForeignKey(oi => oi.OrderId);


            eb.HasOne<Product>()
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .HasPrincipalKey(p => p.Id);


            eb.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            eb.Ignore(oi => oi.Subtotal); // Ignorar subtotal, se calcula en tiempo de ejecución

        });


    }

}
