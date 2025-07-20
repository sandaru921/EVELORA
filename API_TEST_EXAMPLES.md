# API Testing Examples for Quiz Ranking System

This file contains examples of how to test the ranking system API endpoints using curl, Postman, or any HTTP client.

## Base URL
```
http://localhost:5000/api/analysis
```

## 1. Get All Rankings
```bash
curl -X GET "http://localhost:5000/api/analysis/rankings"
```

**Expected Response:**
```json
[
  {
    "rank": 1,
    "name": "Test User",
    "marks": 2,
    "percentage": 40.0,
    "category": "Database",
    "timeTaken": 24
  },
  {
    "rank": 2,
    "name": "Test User",
    "marks": 2,
    "percentage": 40.0,
    "category": "Database",
    "timeTaken": 69
  },
  {
    "rank": 3,
    "name": "Test User",
    "marks": 1,
    "percentage": 16.7,
    "category": "Programming",
    "timeTaken": 15
  }
]
```

## 2. Get Rankings by Category
```bash
curl -X GET "http://localhost:5000/api/analysis/rankings/category/Database"
```

**Expected Response:**
```json
[
  {
    "rank": 1,
    "name": "Test User",
    "marks": 2,
    "percentage": 40.0,
    "category": "Database",
    "timeTaken": 24
  },
  {
    "rank": 2,
    "name": "Test User",
    "marks": 2,
    "percentage": 40.0,
    "category": "Database",
    "timeTaken": 69
  }
]
```

## 3. Get Available Categories
```bash
curl -X GET "http://localhost:5000/api/analysis/categories"
```

**Expected Response:**
```json
[
  "Programming",
  "Database",
  "Web Development",
  "Mobile Development"
]
```

## 4. Get Ranking Statistics
```bash
curl -X GET "http://localhost:5000/api/analysis/statistics"
```

**Expected Response:**
```json
{
  "totalParticipants": 1,
  "averageScore": 26.0,
  "averageTime": 30.2,
  "topScore": 40.0,
  "categories": [
    "Programming",
    "Database",
    "Web Development",
    "Mobile Development"
  ]
}
```

## 5. Submit Quiz Result
```bash
curl -X POST "http://localhost:5000/api/analysis/submitResult" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "8",
    "quizId": 26,
    "score": 3,
    "totalMarks": 5,
    "timeTaken": 45,
    "userIdInt": 0
  }'
```

**Expected Response:**
```json
{
  "id": 15,
  "userId": "8",
  "quizId": 26,
  "score": 3,
  "totalMarks": 5,
  "submissionTime": "2025-01-27T10:30:00.000Z",
  "timeTaken": 45,
  "userIdInt": 0,
  "percentage": 60.0
}
```

## 6. Get User Results
```bash
curl -X GET "http://localhost:5000/api/analysis/user/8/results"
```

**Expected Response:**
```json
[
  {
    "id": 6,
    "userId": "8",
    "quizId": 26,
    "score": 2,
    "totalMarks": 5,
    "submissionTime": "2025-07-18T06:00:00.566562Z",
    "timeTaken": 24,
    "userIdInt": 0,
    "percentage": 40.0
  },
  {
    "id": 7,
    "userId": "8",
    "quizId": 22,
    "score": 1,
    "totalMarks": 6,
    "submissionTime": "2025-07-18T07:20:04.149309Z",
    "timeTaken": 15,
    "userIdInt": 0,
    "percentage": 16.7
  }
]
```

## Postman Collection

You can import this into Postman:

```json
{
  "info": {
    "name": "Quiz Ranking System API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get All Rankings",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/analysis/rankings"
      }
    },
    {
      "name": "Get Rankings by Category",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/analysis/rankings/category/Database"
      }
    },
    {
      "name": "Get Categories",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/analysis/categories"
      }
    },
    {
      "name": "Get Statistics",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/analysis/statistics"
      }
    },
    {
      "name": "Submit Quiz Result",
      "request": {
        "method": "POST",
        "url": "http://localhost:5000/api/analysis/submitResult",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"userId\": \"8\",\n  \"quizId\": 26,\n  \"score\": 3,\n  \"totalMarks\": 5,\n  \"timeTaken\": 45,\n  \"userIdInt\": 0\n}"
        }
      }
    },
    {
      "name": "Get User Results",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/analysis/user/8/results"
      }
    }
  ]
}
```

## Testing with JavaScript/Fetch

```javascript
// Get all rankings
async function getRankings() {
  const response = await fetch('http://localhost:5000/api/analysis/rankings');
  const rankings = await response.json();
  console.log('Rankings:', rankings);
}

// Submit a quiz result
async function submitQuizResult() {
  const result = {
    userId: "8",
    quizId: 26,
    score: 3,
    totalMarks: 5,
    timeTaken: 45,
    userIdInt: 0
  };

  const response = await fetch('http://localhost:5000/api/analysis/submitResult', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(result)
  });

  const submittedResult = await response.json();
  console.log('Submitted result:', submittedResult);
}

// Get statistics
async function getStatistics() {
  const response = await fetch('http://localhost:5000/api/analysis/statistics');
  const stats = await response.json();
  console.log('Statistics:', stats);
}
```

## SignalR Testing

To test real-time updates, you can use the SignalR client:

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/analysisHub")
  .build();

// Listen for quiz result updates
connection.on("ReceiveQuizResult", (result) => {
  console.log("New quiz result received:", result);
  // Update your UI here
});

// Start connection
connection.start().catch(err => console.error(err));
``` 