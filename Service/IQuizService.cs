using AssessmentPlatform.Backend.DTO;
     using AssessmentPlatform.Backend.DTOs;
     using AssessmentPlatform.Backend.Models;
     using System.Collections.Generic;
     using System.Threading.Tasks;

     namespace AssessmentPlatform.Backend.Services
     {
         public interface IQuizService
         {
             Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto);
             Task<Quiz?> GetQuizByIdAsync(int id);
             Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
             Task<QuizResultResponseDto> SaveQuizResultAsync(QuizSubmissionDto submissionDto);
             Task<IEnumerable<QuizResultResponseDto>> GetAllQuizResultsAsync();
             Task<QuizResultResponseDto?> GetQuizResultByIdAsync(int id);
             Task<QuizResultAnswerResponseDto?> GetQuizAnswerByIdAsync(int id);
             Task<QuizResponseDto?> UpdateQuizAsync(int id, QuizUpdateDto updateQuizDto);
             Task<bool> DeleteQuizAsync(int id);
         }
     }