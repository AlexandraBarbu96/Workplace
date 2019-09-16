namespace Workplace_v2._2.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Workplace_v2._2.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Workplace_v2._2.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Workplace_v2._2.Models.ApplicationDbContext context)
        {
            context.Studies.AddOrUpdate(
                new Study() { StudyId = 1, Degree = "Associate's", Specialty = "AA - Associate of Arts" },
                new Study() { StudyId = 2, Degree = "Associate's", Specialty = "AAA - Associate of Applied Arts" },
                new Study() { StudyId = 3, Degree = "Associate's", Specialty = "AE - Associate of Engineering or Associate in Electronics Engineering Technology" },
                new Study() { StudyId = 4, Degree = "Associate's", Specialty = "AS - Associate of Science" },
                new Study() { StudyId = 5, Degree = "Associate's", Specialty = "AGS - Associate of General Studies" },
                new Study() { StudyId = 6, Degree = "Associate's", Specialty = "ASN - Associate of Science in Nursing" },
                new Study() { StudyId = 7, Degree = "Associate's", Specialty = "AF - Associate of Forestry" },
                new Study() { StudyId = 8, Degree = "Associate's", Specialty = "AT - Associate of Technology" },
                new Study() { StudyId = 9, Degree = "Associate's", Specialty = "AAB - Associate of Applied Business" },
                new Study() { StudyId = 10, Degree = "Associate's", Specialty = "AAS - Associate of Applied Science or Associate of Arts and Sciences" },
                new Study() { StudyId = 11, Degree = "Associate's", Specialty = "AAT - Associate of Arts in Teaching" },
                new Study() { StudyId = 12, Degree = "Associate's", Specialty = "ABS - Associate of Baccalaureate Studies" },
                new Study() { StudyId = 13, Degree = "Associate's", Specialty = "ABA - Associate of Business Administration" },
                new Study() { StudyId = 14, Degree = "Associate's", Specialty = "AES - Associate of Engineering Science" },
                new Study() { StudyId = 15, Degree = "Associate's", Specialty = "ADN - Associate Degree in Nursin" },
                new Study() { StudyId = 16, Degree = "Associate's", Specialty = "AET - Associate in Engineering Technology" },
                new Study() { StudyId = 17, Degree = "Associate's", Specialty = "AFA - Associate of Fine Arts" },
                new Study() { StudyId = 18, Degree = "Associate's", Specialty = "APE - Associate of Pre-Engineering" },
                new Study() { StudyId = 19, Degree = "Associate's", Specialty = "AIT - Associate of Industrial Technology" },
                new Study() { StudyId = 20, Degree = "Associate's", Specialty = "AOS - Associate of Occupational Studies" },
                new Study() { StudyId = 21, Degree = "Associate's", Specialty = "ASPT-APT - Associate in Physical Therapy" },
                new Study() { StudyId = 22, Degree = "Associate's", Specialty = "APS - Associate of Political Science or Associate of Public Service" },
                new Study() { StudyId = 23, Degree = "Bachelor's", Specialty = "Bachelor of Architecture (BArch)" },
                new Study() { StudyId = 24, Degree = "Bachelor's", Specialty = "Bachelor of Arts (BA, AB, BS, BSc, SB, ScB)" },
                new Study() { StudyId = 25, Degree = "Bachelor's", Specialty = "Bachelor of Applied Arts (BAA)" },
                new Study() { StudyId = 26, Degree = "Bachelor's", Specialty = "Bachelor of Applied Arts and Science (BAAS)" },
                new Study() { StudyId = 27, Degree = "Bachelor's", Specialty = "Bachelor of Applied Science in Information Technology (BAppSc(IT))" },
                new Study() { StudyId = 28, Degree = "Bachelor's", Specialty = "Bachelor of Design (BDes, or SDes in Indonesia)" },
                new Study() { StudyId = 29, Degree = "Bachelor's", Specialty = "Bachelor of Engineering (BEng, BE, BSE, BESc, BSEng, BASc, BTech, BSc(Eng), AMIE,GradIETE)" },
                new Study() { StudyId = 30, Degree = "Bachelor's", Specialty = "Bachelor of Science in Business (BSBA)" },
                new Study() { StudyId = 31, Degree = "Bachelor's", Specialty = "Bachelor of Engineering Technology (BSET)" },
                new Study() { StudyId = 32, Degree = "Bachelor's", Specialty = "Bachelor of Technology (B.Tech. or B.Tech.)" },
                new Study() { StudyId = 33, Degree = "Bachelor's", Specialty = "Bachelor of International Business Economics (BIBE)" },
                new Study() { StudyId = 34, Degree = "Bachelor's", Specialty = "Bachelor of Business Administration (BBA)" },
                new Study() { StudyId = 35, Degree = "Bachelor's", Specialty = "Bachelor of Management Studies (BMS)" },
                new Study() { StudyId = 36, Degree = "Bachelor's", Specialty = "Bachelor of Administrative Studies" },
                new Study() { StudyId = 37, Degree = "Bachelor's", Specialty = "Bachelor of Commerce (BCom, or BComm)" },
                new Study() { StudyId = 38, Degree = "Bachelor's", Specialty = "Bachelor of Fine Arts (BFA)" },
                new Study() { StudyId = 39, Degree = "Bachelor's", Specialty = "Bachelor of Business (BBus)" },
                new Study() { StudyId = 40, Degree = "Bachelor's", Specialty = "Bachelor of Management and Organizational Studies (BMOS)" },
                new Study() { StudyId = 41, Degree = "Bachelor's", Specialty = "Bachelor of Business Science (BBusSc)" },
                new Study() { StudyId = 42, Degree = "Bachelor's", Specialty = "Bachelor of Accountancy (B.Acy. or B.Acc. or B. Accty)" },
                new Study() { StudyId = 43, Degree = "Bachelor's", Specialty = "Bachelor of Comptrolling (B.Acc.Sci. or B.Compt.)" },
                new Study() { StudyId = 44, Degree = "Bachelor's", Specialty = "Bachelor of Economics (BEc, BEconSc; sometimes BA(Econ) or BSc(Econ))" },
                new Study() { StudyId = 45, Degree = "Bachelor's", Specialty = "Bachelor of Arts in Organizational Management (BAOM)" },
                new Study() { StudyId = 46, Degree = "Bachelor's", Specialty = "Bachelor of Computer Science (BCompSc)" },
                new Study() { StudyId = 47, Degree = "Bachelor's", Specialty = "Bachelor of Computing (BComp)" },
                new Study() { StudyId = 48, Degree = "Bachelor's", Specialty = "Bachelor of Science in Information Technology (BSc IT)" },
                new Study() { StudyId = 49, Degree = "Bachelor's", Specialty = "Bachelor of Computer Applications (BCA)" },
                new Study() { StudyId = 50, Degree = "Bachelor's", Specialty = "Bachelor of Business Information Systems (BBIS)" },
                new Study() { StudyId = 51, Degree = "Master's", Specialty = "Master of Accountancy (MAcc, MAc, or MAcy)" },
                new Study() { StudyId = 52, Degree = "Master's", Specialty = "Master of Advanced Study (MAS)" },
                new Study() { StudyId = 53, Degree = "Master's", Specialty = "Master of Economics (MEcon)" },
                new Study() { StudyId = 54, Degree = "Master's", Specialty = "Master of Architecture (MArch)" },
                new Study() { StudyId = 55, Degree = "Master's", Specialty = "Master of Applied Science (MASc, MAppSc, MApplSc, MASc and MAS)" },
                new Study() { StudyId = 56, Degree = "Master's", Specialty = "Master of Arts (MA, MA, AM, or AM)" },
                new Study() { StudyId = 57, Degree = "Master's", Specialty = "Master of Arts in Teaching (MAT)" },
                new Study() { StudyId = 58, Degree = "Master's", Specialty = "Master of Arts in Liberal Studies (MA, ALM, MLA, MLS or MALS)" },
                new Study() { StudyId = 59, Degree = "Master's", Specialty = "Master of Business (MBus)" },
                new Study() { StudyId = 60, Degree = "Master's", Specialty = "Master of Business Administration (MBA or MBA)" },
                new Study() { StudyId = 61, Degree = "Master's", Specialty = "Master of Business Informatics (MBI)" },
                new Study() { StudyId = 62, Degree = "Master's", Specialty = "Master of City Planning" },
                new Study() { StudyId = 63, Degree = "Master's", Specialty = "Master of Chemistry (MChem)" },
                new Study() { StudyId = 64, Degree = "Master's", Specialty = "Master of Commerce (MCom or MComm)" },
                new Study() { StudyId = 65, Degree = "Master's", Specialty = "Master of Computational Finance (or Quantitative Finance)" },
                new Study() { StudyId = 66, Degree = "Master's", Specialty = "Master of Computer Applications (MCA)" },
                new Study() { StudyId = 67, Degree = "Master's", Specialty = "Master in Creative Technologies" },
                new Study() { StudyId = 68, Degree = "Master's", Specialty = "Master of Criminal Justice (MCJ)" },
                new Study() { StudyId = 69, Degree = "Master's", Specialty = "Master of Design (MDes, MDes or MDesign)" },
                new Study() { StudyId = 70, Degree = "Master's", Specialty = "Master of Economics (MEcon)" },
                new Study() { StudyId = 71, Degree = "Master's", Specialty = "Master of Education (MEd, MEd, EdM, MAEd, MSEd, MSE, or MEdL)" },
                new Study() { StudyId = 72, Degree = "Master's", Specialty = "Master of Enterprise (MEnt)" },
                new Study() { StudyId = 73, Degree = "Master's", Specialty = "Master of Engineering (MEng, ME or MEng)" },
                new Study() { StudyId = 74, Degree = "Doctoral", Specialty = "Doctor of Business Administration (DBA)" },
                new Study() { StudyId = 75, Degree = "Professional", Specialty = "Professional of Dentistry (DDS, DMD)" },
                new Study() { StudyId = 76, Degree = "Specialist", Specialty = "Specialist of Education (Ed.S or Sp.Ed)" }
                );
        }
    }
}
