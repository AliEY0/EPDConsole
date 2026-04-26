using Chipsoft.Assignments.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chipsoft.Assignments.Infrastructure.DataAccess.Configuration;

public class EPDDbContext : DbContext
{
    //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //=> options.UseSqlite($"Data Source=epd.db");

    public EPDDbContext(DbContextOptions<EPDDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(patient => patient.Id);
            entity.Property(patient => patient.Name).IsRequired();
            entity.Property(patient => patient.Address).IsRequired();
            entity.Property(patient => patient.PhoneNumber).IsRequired();
            entity.Property(patient => patient.Email).IsRequired();
            entity.Property(patient => patient.DateOfBirth).IsRequired();
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(doctor => doctor.Id);
            entity.Property(doctor => doctor.Name).IsRequired();
            entity.Property(doctor => doctor.Address).IsRequired();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(appointment => appointment.Id);
            entity.Property(appointment => appointment.Date).IsRequired();

            entity.HasOne(appointment => appointment.Patient)
                .WithMany(patient => patient.Appointments)
                .HasForeignKey(appointment => appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(appointment => appointment.Doctor)
                .WithMany(doctor => doctor.Appointments)
                .HasForeignKey(appointment => appointment.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

}