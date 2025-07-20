using System;
using System.Collections.Generic;
using System.Linq;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Tests
{
    public class RankingTest
    {
        public static void Main()
        {
            // Sample data from the database
            var quizResults = new List<QuizResults>
            {
                new QuizResults { Id = 6, UserId = "8", QuizId = 26, Score = 2, TotalMarks = 5, SubmissionTime = DateTime.Parse("2025-07-18 06:00:00.566562+00"), TimeTaken = 24, UserIdInt = 0 },
                new QuizResults { Id = 7, UserId = "8", QuizId = 22, Score = 1, TotalMarks = 6, SubmissionTime = DateTime.Parse("2025-07-18 07:20:04.149309+00"), TimeTaken = 15, UserIdInt = 0 },
                new QuizResults { Id = 12, UserId = "8", QuizId = 31, Score = 0, TotalMarks = 4, SubmissionTime = DateTime.Parse("2025-07-18 09:51:33.767833+00"), TimeTaken = 14, UserIdInt = 0 },
                new QuizResults { Id = 13, UserId = "8", QuizId = 26, Score = 2, TotalMarks = 5, SubmissionTime = DateTime.Parse("2025-07-18 10:11:29.649726+00"), TimeTaken = 69, UserIdInt = 0 },
                new QuizResults { Id = 14, UserId = "8", QuizId = 32, Score = 1, TotalMarks = 3, SubmissionTime = DateTime.Parse("2025-07-18 10:37:33.535127+00"), TimeTaken = 29, UserIdInt = 0 }
            };

            // Sample quiz categories (you would get this from the database)
            var quizCategories = new Dictionary<int, string>
            {
                { 22, "Programming" },
                { 26, "Database" },
                { 31, "Web Development" },
                { 32, "Mobile Development" }
            };

            // Calculate rankings
            var rankings = quizResults
                .Select(r => new Ranking
                {
                    Name = "Test User", // Hardcoded for now
                    Marks = r.Score,
                    Percentage = r.TotalMarks > 0 ? (double)r.Score / r.TotalMarks * 100 : 0,
                    TimeTaken = r.TimeTaken,
                    Category = quizCategories.ContainsKey(r.QuizId) ? quizCategories[r.QuizId] : "Unknown"
                })
                .OrderByDescending(r => r.Marks)
                .ThenBy(r => r.TimeTaken)
                .ToList();

            // Assign ranks
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
                
                // Handle ties
                if (i > 0 && rankings[i].Marks == rankings[i - 1].Marks && 
                    rankings[i].TimeTaken == rankings[i - 1].TimeTaken)
                {
                    rankings[i].Rank = rankings[i - 1].Rank;
                }
            }

            // Display rankings
            Console.WriteLine("Quiz Rankings:");
            Console.WriteLine("Rank | Name       | Marks | Percentage | Time | Category");
            Console.WriteLine("-----|------------|-------|------------|------|----------");
            
            foreach (var ranking in rankings)
            {
                Console.WriteLine($"{ranking.Rank,4} | {ranking.Name,-10} | {ranking.Marks,5} | {ranking.Percentage,9:F1}% | {ranking.TimeTaken,4} | {ranking.Category}");
            }
        }
    }
} 