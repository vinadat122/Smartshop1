
# SmartShop

SmartShop is an ASP.NET Core Web API application built with .NET 10, Entity Framework Core, MySQL, and Google Gemini AI.

## Technologies
- .NET 10 / ASP.NET Core Web API
- Entity Framework Core
- MySQL
- Google Gemini API
- Razor Pages

## Database
Create a MySQL database named `SMARTSHOP`. Configure the connection string in `appsettings.json`.

```json
{
  "ConnectionStrings": {
    "SmartShop": "Server=localhost;Port=3306;Database=SMARTSHOP;User=root;Password=YOUR_PASSWORD;"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-2.5-flash"
  }
}
```

Replace the placeholders with your local MySQL password and Gemini API key. Do not commit secrets to GitHub.

## Run the application
1. Open `Smartshop1.slnx` in Visual Studio.
2. Configure the database connection string and Gemini API key.
3. Apply EF Core migrations if needed:
   - `dotnet ef database update`
4. Build and run the project in Visual Studio.
5. Open `/Reports` to view the review report.

## API Endpoints

### 1. Generate product description
`POST /api/products/{id}/generatedescription`

Generates a professional product description using Gemini and saves it to the database.

**System prompt:** You are an e-commerce content specialist. Write a professional product description in English with at least 80 words. Use only the supplied product information and do not invent specifications.

**User prompt:** Generate a description for the supplied product name, category, and features. Return only the description text, without a title or introduction.

### 2. Analyze a review
`POST /api/reviews/analyzereview`

Request body:
```json
{
  "productId": 1,
  "customerName": "Alex",
  "content": "The product works well and is easy to use.",
  "rating": 5
}
```

Gemini analyzes the review and returns a sentiment, score, and short summary. The result is saved in the database.

**System prompt:** Analyze a customer review. Return JSON only with `sentiment` (`Positive`, `Negative`, or `Neutral`), `score` (a number from 0 to 1), and `summary` (maximum 256 characters).

**User prompt:** Analyze the following customer review and return the requested JSON fields: `{content}`.

### 3. Product assistant chatbot
`POST /api/assistant/chat`

Request body:
```json
{
  "sessionId": "customer-001",
  "message": "What products are available?"
}
```

The assistant uses product information from the database and recent messages associated with the session ID.

**System prompt:** You are SmartShop's product assistant. Answer only questions about products in the supplied SmartShop data, such as names, prices, categories, features, and descriptions. Use only the supplied data and do not invent product details. Politely refuse unrelated questions. Respond in Vietnamese.

**User prompt:** Answer the customer's question using the supplied product data and conversation history.

### 4. Review report
`GET /api/reports/reviews`

Returns each product's total reviews, positive reviews, negative reviews, neutral reviews, and average rating.

## Frontend
Open `/Reports` to view the product review report table.

## Security
Keep API keys and database passwords private. Use local configuration or environment variables for secrets.
