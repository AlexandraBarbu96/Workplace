namespace Workplace_v2._2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        AnswerId = c.Int(nullable: false, identity: true),
                        CandidateId = c.String(maxLength: 128),
                        QuestionId = c.Int(nullable: false),
                        Text = c.String(),
                        Score = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.CandidateId)
                .Index(t => t.CandidateId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CandidateInterviewId = c.Int(nullable: false),
                        AdminDepartmentId = c.Int(nullable: false),
                        EmployeeDepartmentId = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Department_DepartmentId = c.Int(),
                        AdminDepartment_DepartmentId = c.Int(),
                        EmployeeDepartment_DepartmentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departments", t => t.Department_DepartmentId)
                .ForeignKey("dbo.Departments", t => t.AdminDepartment_DepartmentId)
                .ForeignKey("dbo.Departments", t => t.EmployeeDepartment_DepartmentId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.Department_DepartmentId)
                .Index(t => t.AdminDepartment_DepartmentId)
                .Index(t => t.EmployeeDepartment_DepartmentId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        AdminId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        JobId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(),
                        YearsOfExperience = c.String(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                        MinimumScore = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId, cascadeDelete: true)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Interviews",
                c => new
                    {
                        InterviewId = c.Int(nullable: false, identity: true),
                        IsDone = c.Boolean(nullable: false),
                        IsGraded = c.Boolean(nullable: false),
                        HasBegun = c.Boolean(nullable: false),
                        Score = c.Single(nullable: false),
                        JobId = c.Int(nullable: false),
                        CandidateId = c.String(nullable: false),
                        Candidate_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.InterviewId)
                .ForeignKey("dbo.AspNetUsers", t => t.Candidate_Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId)
                .Index(t => t.Candidate_Id);
            
            CreateTable(
                "dbo.JobSkills",
                c => new
                    {
                        JobSkillId = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        SkillId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobSkillId)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.Skills", t => t.SkillId, cascadeDelete: true)
                .Index(t => t.JobId)
                .Index(t => t.SkillId);
            
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        SkillId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.SkillId);
            
            CreateTable(
                "dbo.CandidateSkills",
                c => new
                    {
                        CandidateSkillId = c.Int(nullable: false, identity: true),
                        ExperienceId = c.Int(nullable: false),
                        SkillId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CandidateSkillId)
                .ForeignKey("dbo.Experiences", t => t.ExperienceId, cascadeDelete: true)
                .ForeignKey("dbo.Skills", t => t.SkillId, cascadeDelete: true)
                .Index(t => t.ExperienceId)
                .Index(t => t.SkillId);
            
            CreateTable(
                "dbo.Experiences",
                c => new
                    {
                        ExperienceId = c.Int(nullable: false, identity: true),
                        CandidateId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ExperienceId)
                .ForeignKey("dbo.AspNetUsers", t => t.CandidateId, cascadeDelete: true)
                .Index(t => t.CandidateId);
            
            CreateTable(
                "dbo.Educations",
                c => new
                    {
                        EducationId = c.Int(nullable: false, identity: true),
                        Institution = c.String(),
                        StudyId = c.Int(nullable: false),
                        ExperienceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EducationId)
                .ForeignKey("dbo.Experiences", t => t.ExperienceId, cascadeDelete: true)
                .ForeignKey("dbo.Studies", t => t.StudyId, cascadeDelete: true)
                .Index(t => t.StudyId)
                .Index(t => t.ExperienceId);
            
            CreateTable(
                "dbo.Studies",
                c => new
                    {
                        StudyId = c.Int(nullable: false, identity: true),
                        Degree = c.String(),
                        Specialty = c.String(),
                    })
                .PrimaryKey(t => t.StudyId);
            
            CreateTable(
                "dbo.JobStudies",
                c => new
                    {
                        JobStudyId = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        StudyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobStudyId)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.Studies", t => t.StudyId, cascadeDelete: true)
                .Index(t => t.JobId)
                .Index(t => t.StudyId);
            
            CreateTable(
                "dbo.PastJobs",
                c => new
                    {
                        PastJobId = c.Int(nullable: false, identity: true),
                        Company = c.String(),
                        Position = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ExperienceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PastJobId)
                .ForeignKey("dbo.Experiences", t => t.ExperienceId, cascadeDelete: true)
                .Index(t => t.ExperienceId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        Approved = c.Boolean(nullable: false),
                        JobId = c.Int(nullable: false),
                        CreatorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId)
                .Index(t => t.CreatorId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "EmployeeDepartment_DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Answers", "CandidateId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "AdminDepartment_DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Questions", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Questions", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Answers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.JobSkills", "SkillId", "dbo.Skills");
            DropForeignKey("dbo.CandidateSkills", "SkillId", "dbo.Skills");
            DropForeignKey("dbo.PastJobs", "ExperienceId", "dbo.Experiences");
            DropForeignKey("dbo.JobStudies", "StudyId", "dbo.Studies");
            DropForeignKey("dbo.JobStudies", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Educations", "StudyId", "dbo.Studies");
            DropForeignKey("dbo.Educations", "ExperienceId", "dbo.Experiences");
            DropForeignKey("dbo.CandidateSkills", "ExperienceId", "dbo.Experiences");
            DropForeignKey("dbo.Experiences", "CandidateId", "dbo.AspNetUsers");
            DropForeignKey("dbo.JobSkills", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Interviews", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Interviews", "Candidate_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Jobs", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.AspNetUsers", "Department_DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Departments", "AdminId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Questions", new[] { "CreatorId" });
            DropIndex("dbo.Questions", new[] { "JobId" });
            DropIndex("dbo.PastJobs", new[] { "ExperienceId" });
            DropIndex("dbo.JobStudies", new[] { "StudyId" });
            DropIndex("dbo.JobStudies", new[] { "JobId" });
            DropIndex("dbo.Educations", new[] { "ExperienceId" });
            DropIndex("dbo.Educations", new[] { "StudyId" });
            DropIndex("dbo.Experiences", new[] { "CandidateId" });
            DropIndex("dbo.CandidateSkills", new[] { "SkillId" });
            DropIndex("dbo.CandidateSkills", new[] { "ExperienceId" });
            DropIndex("dbo.JobSkills", new[] { "SkillId" });
            DropIndex("dbo.JobSkills", new[] { "JobId" });
            DropIndex("dbo.Interviews", new[] { "Candidate_Id" });
            DropIndex("dbo.Interviews", new[] { "JobId" });
            DropIndex("dbo.Jobs", new[] { "DepartmentId" });
            DropIndex("dbo.Departments", new[] { "AdminId" });
            DropIndex("dbo.AspNetUsers", new[] { "EmployeeDepartment_DepartmentId" });
            DropIndex("dbo.AspNetUsers", new[] { "AdminDepartment_DepartmentId" });
            DropIndex("dbo.AspNetUsers", new[] { "Department_DepartmentId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Answers", new[] { "QuestionId" });
            DropIndex("dbo.Answers", new[] { "CandidateId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Questions");
            DropTable("dbo.PastJobs");
            DropTable("dbo.JobStudies");
            DropTable("dbo.Studies");
            DropTable("dbo.Educations");
            DropTable("dbo.Experiences");
            DropTable("dbo.CandidateSkills");
            DropTable("dbo.Skills");
            DropTable("dbo.JobSkills");
            DropTable("dbo.Interviews");
            DropTable("dbo.Jobs");
            DropTable("dbo.Departments");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Answers");
        }
    }
}
