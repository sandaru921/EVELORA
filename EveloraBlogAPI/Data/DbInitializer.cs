using EverolaBlogAPI.Models;
using System;
using System.Linq;

namespace EverolaBlogAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Check if there are any blogs already
            if (context.Blogs.Any())
            {
                return; // DB has been seeded
            }

            var blogs = new Blog[]
            {
                new Blog
                {
                    Title = "How To Tackle Security Testing And Challenges",
                    Content = @"<h2>Overview</h2>
<p>Cyber security ventures stated that cyber crime is expected to grow by 15% every year. They predicted that those crimes might cost $10.5 trillion annually on a global scale by the year 2025. Security testing plays a vital role in preventing most of these crimes.</p>
<p>In this article, let's look at five security testing challenges and how to tackle them.</p>
<ul>
  <li><strong>Challenge 1:</strong> Focusing on security requirements in the requirements phase</li>
  <li><strong>Challenge 2:</strong> Writing test cases for security testing and updating them regularly</li>
  <li><strong>Challenge 3:</strong> Including API security</li>
  <li><strong>Challenge 4:</strong> Security testing skill set</li>
  <li><strong>Challenge 5:</strong> Selection of tools</li>
</ul>

<h2>Challenge 1: Focusing on security requirements in the requirements phase</h2>
<p>Generally, security requirements are not given equal importance as functional requirements. Sometimes security requirements are considered post-developing certain modules and, in some cases, it is before pre-release. Not defining the security requirements prior makes security testing difficult.</p>
<p><strong>Solution:</strong></p>
<ul>
  <li>Ask, clarify, and document security-related requirement questions for each story/feature in the requirement-defining phase of SDLC.</li>
  <li>Document security requirements before the design phase</li>
  <li>Identify, analyze, and prioritize security risk and mitigation plans.</li>
  <li>Motivate team members to have an agile mindset toward security testing.</li>
</ul>",
                    Category = "Software",
                    ImageUrl = "/images/blogs/security-testing.jpg",
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2023, 4, 7), DateTimeKind.Utc)
                },
                new Blog
                {
                    Title = "Agile Testing: It's a new age of testing",
                    Content = @"<h2>Overview</h2>
<p>Agile methodology has revolutionized the way we approach testing in software development. This article discusses how agile testing differs from traditional methods and why it's becoming the preferred approach for modern development teams.</p>

<h2>What is Agile Testing?</h2>
<p>Agile testing is a software testing practice that follows the principles of agile software development. Unlike the traditional Waterfall method where testing is done only after the build phase is complete, testing and development are concurrent in Agile.</p>

<h2>Key Principles of Agile Testing</h2>
<ul>
  <li><strong>Principle 1:</strong> Testing is not a phase but an ongoing activity</li>
  <li><strong>Principle 2:</strong> Everyone on the team is responsible for quality</li>
  <li><strong>Principle 3:</strong> Continuous feedback is essential</li>
  <li><strong>Principle 4:</strong> Face-to-face communication over documentation</li>
  <li><strong>Principle 5:</strong> Respond to change rather than following a plan</li>
</ul>

<h2>Benefits of Agile Testing</h2>
<p>Implementing agile testing methodologies can lead to faster delivery, improved quality, and better collaboration between team members. It allows for early detection of defects, reducing the cost of fixing them later in the development cycle.</p>",
                    Category = "Software",
                    ImageUrl = "/images/blogs/agile-testing.jpg",
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2022, 11, 23), DateTimeKind.Utc)
                },
                new Blog
                {
                    Title = "CIA Triad of Security – Why does it matter?",
                    Content = @"<h2>Overview</h2>
<p>The CIA triad is a model designed to guide policies for information security within an organization. The model is also sometimes referred to as the AIC triad to avoid confusion with the Central Intelligence Agency.</p>

<h2>What is the CIA Triad?</h2>
<p>The CIA triad consists of three components:</p>
<ul>
  <li><strong>Confidentiality:</strong> Ensuring that information is not made available or disclosed to unauthorized individuals, entities, or processes.</li>
  <li><strong>Integrity:</strong> Maintaining and assuring the accuracy and completeness of data over its entire lifecycle.</li>
  <li><strong>Availability:</strong> Ensuring that information is accessible to authorized users when needed.</li>
</ul>

<h2>Why Does It Matter?</h2>
<p>The CIA triad is important because it helps organizations identify and address security vulnerabilities. By focusing on these three aspects, companies can develop comprehensive security policies that protect their data from various threats.</p>",
                    Category = "Quality Assurance",
                    ImageUrl = "/images/blogs/cia-triad.jpg",
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2023, 6, 26), DateTimeKind.Utc)
                }
            };

            // Generate slugs for each blog
            foreach (var blog in blogs)
            {
                blog.GenerateSlug();
            }

            context.Blogs.AddRange(blogs);
            context.SaveChanges();
        }
    }
}
