using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NoteApp.Data;
using NoteApp.Models;

namespace NoteApp.Services
{
    public class BlogPostService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly NotesContext _context;

        public BlogPostService(HttpClient httpClient, IConfiguration configuration, NotesContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;

            var apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Console.WriteLine("API Key: " + apiKey);
        }

        public async Task<BlogPost> GenerateAndSaveBlogPostAsync(IEnumerable<Note> notes)
        {
            var content = string.Join("\n", notes.Select(n => n.Content));
            var message = new
            {
                role = "user",
                content = $"Create a blog post from the following notes:\n{content}"
            };

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[] { message },
                max_tokens = 500,
                temperature = 0.7
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            Console.WriteLine("Request content: " + await requestContent.ReadAsStringAsync());

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            Console.WriteLine(response.StatusCode);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatGptResponse>(responseContent);

            var blogPost = new BlogPost
            {
                Content = result.choices.First().message.content,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();

            return blogPost;
        }

        public class ChatGptResponse
        {
            public List<Choice> choices { get; set; } = new List<Choice>();

            public class Choice
            {
                public Message message { get; set; } = new Message();
            }

            public class Message
            {
                public string content { get; set; } = string.Empty;
            }
        }
    }
}
