# Quiz Ranking System Implementation

This document explains the implementation of the quiz ranking system for the Assessment Platform.

## Overview

The ranking system calculates user rankings based on:
1. **Marks/Score** (Primary sorting criteria - higher is better)
2. **Time Duration** (Secondary sorting criteria - lower is better)
3. **Quiz Category** (Displayed but not used for ranking)

## Database Schema

The `QuizResults` table contains:
- `Id`: Primary key
- `UserId`: User identifier (currently hardcoded as "8" for "Test User")
- `QuizId`: Quiz identifier
- `Score`: Marks obtained
- `TotalMarks`: Total possible marks
- `SubmissionTime`: When the quiz was submitted
- `TimeTaken`: Time taken in seconds
- `UserIdInt`: Integer user ID for foreign key relationship

## API Endpoints

### 1. Get All Rankings
```
GET /api/analysis/rankings
```
Returns all quiz results ranked by marks and time duration.

### 2. Get Rankings by Category
```
GET /api/analysis/rankings/category/{category}
```
Returns rankings filtered by quiz category.

### 3. Get Available Categories
```
GET /api/analysis/categories
```
Returns list of all available quiz categories.

### 4. Get Ranking Statistics
```
GET /api/analysis/statistics
```
Returns overall statistics including:
- Total participants
- Average score
- Average time
- Top score
- Available categories

### 5. Submit Quiz Result
```
POST /api/analysis/submitResult
```
Submits a new quiz result and updates rankings.

## Sample Data

Based on your PostgreSQL data:
```
Id | UserId | QuizId | Score | TotalMarks | SubmissionTime           | TimeTaken | UserIdInt
6  | "8"    | 26     | 2     | 5          | 2025-07-18 06:00:00     | 24        | 0
7  | "8"    | 22     | 1     | 6          | 2025-07-18 07:20:04     | 15        | 0
12 | "8"    | 31     | 0     | 4          | 2025-07-18 09:51:33     | 14        | 0
13 | "8"    | 26     | 2     | 5          | 2025-07-18 10:11:29     | 69        | 0
14 | "8"    | 32     | 1     | 3          | 2025-07-18 10:37:33     | 29        | 0
```

## Ranking Algorithm

1. **Primary Sort**: By marks (descending)
2. **Secondary Sort**: By time taken (ascending)
3. **Tie Handling**: Same rank for identical marks and time
4. **Percentage Calculation**: (Score / TotalMarks) * 100

## Expected Ranking Output

Based on the sample data:
```
Rank | Name       | Marks | Percentage | Time | Category
1    | Test User  | 2     | 40.0%      | 24   | Database
2    | Test User  | 2     | 40.0%      | 69   | Database
3    | Test User  | 1     | 16.7%      | 15   | Programming
4    | Test User  | 1     | 33.3%      | 29   | Mobile Development
5    | Test User  | 0     | 0.0%       | 14   | Web Development
```

## Files Modified/Created

### Models
- `Models/QuizResults.cs` - Updated with navigation properties
- `Models/User.cs` - Added QuizResults collection
- `Models/Quiz.cs` - Added QuizResults collection

### Services
- `Service/QuizResultRepository.cs` - Enhanced with new methods
- `Service/IQuizResultRepository.cs` - Updated interface
- `Service/QuizAnalysisService.cs` - Complete ranking logic implementation

### Controllers
- `Controllers/AnalysisController.cs` - New ranking endpoints

### Configuration
- `Program.cs` - Service registrations
- `Data/AppDbContext.cs` - Foreign key relationships

## Current Limitations

1. **Hardcoded User**: User ID "8" is hardcoded as "Test User" since the full user system isn't integrated
2. **Category Mapping**: Quiz categories are fetched from the Quiz table, but may show "Unknown" if quiz doesn't exist
3. **Single User**: Currently only shows results for one user

## Future Enhancements

When the full system is integrated:

1. **Dynamic User Names**: Replace hardcoded mapping with actual user data
2. **Multiple Users**: Support for multiple users taking quizzes
3. **Real-time Updates**: SignalR integration for live ranking updates
4. **Advanced Filtering**: Filter by date range, user groups, etc.
5. **Performance Optimization**: Caching and database indexing

## Testing

Run the application and test the endpoints:

```bash
# Start the application
dotnet run

# Test endpoints (using curl or Postman)
curl http://localhost:5000/api/analysis/rankings
curl http://localhost:5000/api/analysis/categories
curl http://localhost:5000/api/analysis/statistics
```

## Database Migration

If you need to update the database schema:

```bash
dotnet ef migrations add UpdateQuizResultsWithNavigation
dotnet ef database update
```

## SignalR Integration

The system includes SignalR for real-time updates. When a new quiz result is submitted, all connected clients receive the updated ranking data automatically. 