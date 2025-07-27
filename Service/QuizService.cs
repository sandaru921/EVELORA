using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssessmentPlatform.Backend.DTO;

namespace AssessmentPlatform.Backend.Services
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuizService(AppDbContext context, ILogger<QuizService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Starting quiz creation for: {QuizName}", createQuizDto.QuizName);

                var quiz = new Quiz
                {
                    QuizName = createQuizDto.QuizName?.Trim(),
                    JobCategory = createQuizDto.JobCategory?.Trim(),
                    Description = createQuizDto.Description?.Trim(),
                    QuizDuration = createQuizDto.QuizDuration,
                    QuizLevel = createQuizDto.QuizLevel?.Trim(),
                    Questions = new List<Question>()
                };

                _logger.LogInformation("Created quiz object, adding {QuestionCount} questions", createQuizDto.Questions.Count);

                foreach (var questionDto in createQuizDto.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = questionDto.QuestionText?.Trim(),
                        CodeSnippet = questionDto.CodeSnippet?.Trim(),
                        ImageURL = questionDto.ImageURL?.Trim(),
                        Type = questionDto.Type?.Trim(),
                        Marks = questionDto.Marks,
                        CorrectAnswers = questionDto.CorrectAnswers ?? new List<string>(),
                        Options = new List<Option>()
                    };

                    if (questionDto.Options != null)
                    {
                        foreach (var optionDto in questionDto.Options)
                        {
                            var option = new Option
                            {
                                Key = optionDto.Key?.Trim(),
                                Value = optionDto.Value?.Trim()
                            };
                            question.Options.Add(option);
                        }
                    }

                    quiz.Questions.Add(question);
                }

                _context.Quizzes.Add(quiz);

                _logger.LogInformation("Saving quiz to database");
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Quiz saved successfully with ID: {QuizId}", quiz.Id);

                return new QuizResponseDto
                {
                    Id = quiz.Id,
                    QuizName = quiz.QuizName,
                    JobCategory = quiz.JobCategory,
                    Description = quiz.Description,
                    QuizDuration = quiz.QuizDuration,
                    QuizLevel = quiz.QuizLevel,
                    QuestionCount = quiz.Questions.Count,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating quiz: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz by ID: {QuizId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Questions)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                throw;
            }
        }

        public async Task<QuizResultResponseDto> SaveQuizResultAsync(QuizSubmissionDto submissionDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Processing quiz submission for Quiz ID: {QuizId}, User ID: {UserId}",
                    submissionDto.QuizId, submissionDto.UserId);

                // Retrieve quiz with questions and options (remove CorrectAnswers from Include)
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == submissionDto.QuizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found for ID: {QuizId}", submissionDto.QuizId);
                    throw new ArgumentException("Quiz not found.");
                }

                // Initialize quiz result
                var quizResult = new QuizResult
                {
                    UserId = submissionDto.UserId,
                    QuizId = submissionDto.QuizId,
                    SubmissionTime = DateTime.UtcNow,
                    TimeTaken = submissionDto.TimeTaken,
                    Answers = new List<Answer>(),
                    TotalMarks = quiz.Questions.Sum(q => q.Marks)
                };

                int score = 0;

                if (submissionDto.Answers != null && submissionDto.Answers.Any())
                {
                    foreach (var answerDto in submissionDto.Answers)
                    {
                        var question = quiz.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                        if (question == null)
                        {
                            _logger.LogWarning("Question not found for ID: {QuestionId}", answerDto.QuestionId);
                            throw new ArgumentException($"Question ID {answerDto.QuestionId} not found.");
                        }

                        var validOptionKeys = question.Options.Select(o => o.Key).ToList();
                        var invalidOptions = answerDto.SelectedOptions.Where(o => !validOptionKeys.Contains(o)).ToList();
                        if (invalidOptions.Any())
                        {
                            _logger.LogWarning("Invalid options submitted for Question ID: {QuestionId}: {InvalidOptions}",
                                answerDto.QuestionId, string.Join(", ", invalidOptions));
                            throw new ArgumentException($"Invalid options for question {answerDto.QuestionId}: {string.Join(", ", invalidOptions)}");
                        }

                        bool isCorrect = question.CorrectAnswers.OrderBy(a => a).SequenceEqual(answerDto.SelectedOptions.OrderBy(o => o));
                        int marksObtained = isCorrect ? question.Marks : 0;
                        score += marksObtained;

                        quizResult.Answers.Add(new Answer
                        {
                            QuestionId = answerDto.QuestionId,
                            SelectedOptions = answerDto.SelectedOptions,
                            IsCorrect = isCorrect,
                            MarksObtained = marksObtained
                        });
                    }
                }

                quizResult.Score = score;

                _context.QuizResults.Add(quizResult);
                _logger.LogInformation("Saving quiz result to database for Quiz ID: {QuizId}", quizResult.QuizId);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Quiz result saved successfully with ID: {ResultId}, Score: {Score}/{TotalMarks}",
                    quizResult.Id, quizResult.Score, quizResult.TotalMarks);

                return new QuizResultResponseDto
                {
                    Id = quizResult.Id,
                    UserId = quizResult.UserId,
                    QuizId = quizResult.QuizId,
                    Score = quizResult.Score,
                    TotalMarks = quizResult.TotalMarks,
                    SubmissionTime = quizResult.SubmissionTime,
                    TimeTaken = quizResult.TimeTaken
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving quiz result: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<QuizResultResponseDto>> GetAllQuizResultsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all quiz results");

                var quizResults = await _context.QuizResults
                    .Include(qr => qr.Answers)
                    .ToListAsync();

                var quizResultDtos = quizResults.Select(qr => new QuizResultResponseDto
                {
                    Id = qr.Id,
                    UserId = qr.UserId,
                    QuizId = qr.QuizId,
                    Score = qr.Score,
                    TotalMarks = qr.TotalMarks,
                    SubmissionTime = qr.SubmissionTime,
                    TimeTaken = qr.TimeTaken
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} quiz results", quizResultDtos.Count);

                return quizResultDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quiz results");
                throw;
            }
        }

        public async Task<QuizResultResponseDto?> GetQuizResultByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving quiz result with ID: {ResultId}", id);

                var quizResult = await _context.QuizResults
                    .Include(qr => qr.Answers)
                    .FirstOrDefaultAsync(qr => qr.Id == id);

                if (quizResult == null)
                {
                    _logger.LogWarning("Quiz result not found for ID: {ResultId}", id);
                    return null;
                }

                var quizResultDto = new QuizResultResponseDto
                {
                    Id = quizResult.Id,
                    UserId = quizResult.UserId,
                    QuizId = quizResult.QuizId,
                    Score = quizResult.Score,
                    TotalMarks = quizResult.TotalMarks,
                    SubmissionTime = quizResult.SubmissionTime,
                    TimeTaken = quizResult.TimeTaken
                };

                _logger.LogInformation("Successfully retrieved quiz result with ID: {ResultId}", id);

                return quizResultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz result with ID: {ResultId}", id);
                throw;
            }
        }

        public async Task<QuizResultAnswerResponseDto?> GetQuizAnswerByIdAsync(int id)
        {
            var answers = await _context.Answers
                .Where(a => a.QuizResultId == id)
                .Select(a => new AnswerDto
                {
                    QuestionId = a.QuestionId,
                    SelectedOptions = a.SelectedOptions.ToList(), // TEXT[] maps to List<string>
                    IsCorrect = a.IsCorrect,
                    MarksObtained = a.MarksObtained
                })
                .ToListAsync();

            if (answers == null || !answers.Any())
                return null;

            return new QuizResultAnswerResponseDto
            {
                QuizResultId = id,
                Answers = answers
            };
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
        {
            return false;
        }

        var questions = _context.Questions.Where(q => q.QuizId == quizId).ToList();

        foreach (var question in questions)
        {
            int questionId = question.Id;

            var answers = _context.Answers.Where(a => a.QuestionId == questionId);
            _context.Answers.RemoveRange(answers);

            var options = _context.Options.Where(o => o.QuestionId == questionId);
            _context.Options.RemoveRange(options);
        }

        _context.Questions.RemoveRange(questions);
        _context.Quizzes.Remove(quiz);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}


        public async Task<QuizResponseDto?> UpdateQuizAsync(int id, QuizUpdateDto updateQuizDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Starting quiz update for ID: {QuizId}", id);

                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz not found for ID: {QuizId}", id);
                    return null;
                }

                // Check for associated answers to prevent question deletion
                var questionIds = quiz.Questions.Select(q => q.Id).ToList();
                var hasAnswers = await _context.Answers.AnyAsync(a => questionIds.Contains(a.QuestionId));
                if (hasAnswers && updateQuizDto.Questions.Count < quiz.Questions.Count)
                {
                    _logger.LogWarning("Cannot update quiz ID {QuizId}: Some questions have associated answers", id);
                    throw new InvalidOperationException("Cannot remove questions that have associated answers.");
                }

                // Update quiz properties
                quiz.QuizName = updateQuizDto.QuizName?.Trim();
                quiz.JobCategory = updateQuizDto.JobCategory?.Trim();
                quiz.Description = updateQuizDto.Description?.Trim();
                quiz.QuizDuration = updateQuizDto.QuizDuration;
                quiz.QuizLevel = updateQuizDto.QuizLevel?.Trim();

                // Update or add questions
                var questionIdsInDto = updateQuizDto.Questions.Where(q => q.Id > 0).Select(q => q.Id).ToList();
                var questionsToRemove = quiz.Questions.Where(q => !questionIdsInDto.Contains(q.Id)).ToList();
                foreach (var questionToRemove in questionsToRemove)
                {
                    var hasQuestionAnswers = await _context.Answers.AnyAsync(a => a.QuestionId == questionToRemove.Id);
                    if (hasQuestionAnswers)
                    {
                        _logger.LogWarning("Cannot remove question ID {QuestionId} because it has associated answers", questionToRemove.Id);
                        throw new InvalidOperationException($"Cannot remove question ID {questionToRemove.Id} because it has associated answers.");
                    }
                    _context.Options.RemoveRange(questionToRemove.Options);
                    _context.Questions.Remove(questionToRemove);
                }

                foreach (var questionDto in updateQuizDto.Questions)
                {
                    var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (existingQuestion != null)
                    {
                        // Update existing question
                        existingQuestion.QuestionText = questionDto.QuestionText?.Trim();
                        existingQuestion.CodeSnippet = questionDto.CodeSnippet?.Trim();
                        existingQuestion.ImageURL = questionDto.ImageURL?.Trim();
                        existingQuestion.Type = questionDto.Type?.Trim();
                        existingQuestion.Marks = questionDto.Marks;
                        existingQuestion.CorrectAnswers = questionDto.CorrectAnswers ?? new List<string>();

                        // Update or add options
                        var optionIdsInDto = questionDto.Options.Where(o => o.Id > 0).Select(o => o.Id).ToList();
                        var optionsToRemove = existingQuestion.Options.Where(o => !optionIdsInDto.Contains(o.Id)).ToList();
                        foreach (var optionToRemove in optionsToRemove)
                        {
                            _context.Options.Remove(optionToRemove);
                        }

                        foreach (var optionDto in questionDto.Options)
                        {
                            var existingOption = existingQuestion.Options.FirstOrDefault(o => o.Id == optionDto.Id);
                            if (existingOption != null)
                            {
                                existingOption.Key = optionDto.Key?.Trim();
                                existingOption.Value = optionDto.Value?.Trim();
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(optionDto.Key) || string.IsNullOrWhiteSpace(optionDto.Value))
                                {
                                    _logger.LogWarning("Invalid option for question ID {QuestionId}: Key: {Key}, Value: {Value}",
                                        existingQuestion.Id, optionDto.Key, optionDto.Value);
                                    throw new ArgumentException($"Invalid option for question ID {existingQuestion.Id}");
                                }
                                existingQuestion.Options.Add(new Option
                                {
                                    Key = optionDto.Key?.Trim(),
                                    Value = optionDto.Value?.Trim()
                                });
                            }
                        }
                    }
                    else
                    {
                        // Add new question
                        var newQuestion = new Question
                        {
                            QuestionText = questionDto.QuestionText?.Trim(),
                            CodeSnippet = questionDto.CodeSnippet?.Trim(),
                            ImageURL = questionDto.ImageURL?.Trim(),
                            Type = questionDto.Type?.Trim(),
                            Marks = questionDto.Marks,
                            CorrectAnswers = questionDto.CorrectAnswers ?? new List<string>(),
                            Options = new List<Option>()
                        };

                        if (questionDto.Options != null)
                        {
                            foreach (var optionDto in questionDto.Options)
                            {
                                if (string.IsNullOrWhiteSpace(optionDto.Key) || string.IsNullOrWhiteSpace(optionDto.Value))
                                {
                                    _logger.LogWarning("Invalid option for new question: {QuestionText}, Key: {Key}, Value: {Value}",
                                        questionDto.QuestionText, optionDto.Key, optionDto.Value);
                                    throw new ArgumentException($"Invalid option for question: {questionDto.QuestionText}");
                                }
                                newQuestion.Options.Add(new Option
                                {
                                    Key = optionDto.Key?.Trim(),
                                    Value = optionDto.Value?.Trim()
                                });
                            }
                        }

                        quiz.Questions.Add(newQuestion);
                    }
                }

                _context.Quizzes.Update(quiz);
                _logger.LogInformation("Saving updated quiz to database for ID: {QuizId}", id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Quiz updated successfully with ID: {QuizId}", id);

                return new QuizResponseDto
                {
                    Id = quiz.Id,
                    QuizName = quiz.QuizName,
                    JobCategory = quiz.JobCategory,
                    Description = quiz.Description,
                    QuizDuration = quiz.QuizDuration,
                    QuizLevel = quiz.QuizLevel,
                    QuestionCount = quiz.Questions.Count,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating quiz with ID: {QuizId}: {Message}, InnerException: {InnerMessage}, StackTrace: {StackTrace}",
                    id, ex.Message, ex.InnerException?.Message, ex.StackTrace);
                throw;
            }
        }
    }
}