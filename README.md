# 🧠 AI Document Intelligence Platform

A full RAG (Retrieval-Augmented Generation) pipeline built with **Clean Architecture**, running entirely locally using **Ollama**, **Qdrant**, and **PostgreSQL**.

```
Upload PDF → Extract Text → Chunk → Embed (Ollama) → Store (Qdrant)
Ask Question → Embed Query → Semantic Search → LLM Answer (llama3)
```

---

## 🏗️ Architecture

```
AiDocAnalyzer/
├── Api/                    # ASP.NET Core Web API (controllers, middleware, DI)
│   ├── Controllers/
│   │   ├── DocumentsController.cs  # /documents/upload | /search | /ask
│   │   ├── HealthController.cs     # /health | /health/vector
│   │   └── TestController.cs       # /test/embed
│   └── Middleware/
│       └── GlobalExceptionMiddleware.cs
│
├── Application/            # Business logic (MediatR commands/queries)
│   ├── Documents/
│   │   ├── Commands/UploadDocumentCommand.cs
│   │   └── Queries/SearchDocumentsQuery.cs | AskDocumentQuery.cs
│   └── Common/
│       ├── Interfaces/     # Contracts (IVectorDbService, IEmbeddingService, etc.)
│       └── DTOs/           # Request/response models
│
├── Domain/                 # Entities (pure C#, no dependencies)
│   └── Entities/Document.cs | DocumentChunk.cs
│
└── Infrastructure/         # External implementations
    ├── VectorDb/QdrantVectorDbService.cs
    ├── Embedding/OllamaEmbeddingService.cs | OllamaLlmService.cs
    ├── TextExtraction/PdfTextExtractor.cs | TextChunker.cs
    └── Persistence/AppDbContext.cs | DocumentRepository.cs
```

---

## 🚀 Quick Start

### Phase 0 — Prerequisites

```bash
# Verify installs
dotnet --version   # Must be 8.x
docker --version
ollama --version
git --version
```

### Phase 1 — Start Services

```bash
# Start Qdrant
docker run -d -p 6333:6333 -p 6334:6334 --name qdrant qdrant/qdrant

# Pull Ollama models
ollama pull nomic-embed-text
ollama pull llama3

# Start PostgreSQL
docker run -d -p 5432:5432 \
  -e POSTGRES_DB=aidocanalyzer \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  --name postgres postgres:16-alpine
```

### Phase 2 — Run the API

```bash
cd AiDocAnalyzer/Api
dotnet run
# API starts at https://localhost:5001 / Swagger at http://localhost:5000
```

### Phase 3 — Add EF Migrations

```bash
cd AiDocAnalyzer
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api
```

---

## 🐳 Docker Compose (Full Stack)

```bash
# Build and start everything
docker compose up --build

# Services:
#   API        → http://localhost:80
#   Qdrant     → http://localhost:6333
#   Ollama     → http://localhost:11434
#   PostgreSQL → localhost:5432
```

---

## 📡 API Endpoints

### Health
```
GET  /health           → API alive
GET  /health/vector    → Qdrant connectivity
GET  /healthz          → Full health check
```

### Testing
```
POST /test/embed
Body: { "text": "Hello world" }
Response: { "vectorLength": 768, "model": "nomic-embed-text" }
```

### Documents
```
POST /documents/upload
  Content-Type: multipart/form-data
  file: <PDF or TXT file>

POST /documents/search
  Body: { "query": "What are the main findings?", "topK": 3 }

POST /documents/ask
  Body: { "question": "What does the document say about X?", "topK": 3 }
```

---

## 🔧 Configuration

Edit `Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aidocanalyzer;Username=postgres;Password=postgres"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text",
    "LlmModel": "llama3"
  },
  "Qdrant": {
    "Host": "localhost",
    "Port": 6334
  }
}
```

---

## 🔄 Pipeline Flow

```
1. POST /documents/upload (PDF)
   └─ PdfTextExtractor    → raw text
   └─ TextChunker         → 600-char chunks with 100-char overlap
   └─ OllamaEmbeddingService → float[768] per chunk (nomic-embed-text)
   └─ QdrantVectorDbService  → upsert to "documents" collection
   └─ DocumentRepository     → save metadata to PostgreSQL

2. POST /documents/ask
   └─ OllamaEmbeddingService → embed question
   └─ QdrantVectorDbService  → cosine search, top-3 chunks
   └─ OllamaLlmService       → llama3 with strict RAG prompt
   └─ Return answer + sources
```

---

## 🏭 Production Features (Phase 9)

- ✅ Serilog structured logging (console + rolling file)
- ✅ Global exception middleware
- ✅ Rate limiting (60 req/min per IP)
- ✅ CORS
- ✅ Swagger / OpenAPI docs
- ✅ Health checks (`/healthz`)
- ✅ Docker multi-stage build
- ✅ EF Core Code-First migrations

### To add JWT Authentication

```csharp
// In Program.cs, add:
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
app.UseAuthentication();
// Then decorate controllers with [Authorize]
```

---

## 📦 Tech Stack

| Component | Technology |
|-----------|-----------|
| API | ASP.NET Core 8 |
| Architecture | Clean Architecture + CQRS (MediatR) |
| Vector DB | Qdrant |
| Embeddings | Ollama (nomic-embed-text) |
| LLM | Ollama (llama3) |
| Metadata DB | PostgreSQL + EF Core |
| Logging | Serilog |
| Containerization | Docker + Docker Compose |
| PDF Parsing | UglyToad.PdfPig |
