using GenAlpha.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface IAiTutorService
    {
        Task<AiResponseDto> ExplainConceptAsync(AiExplainRequestDto request);
        Task<AiResponseDto> SummarizeLessonAsync(AiSummarizeRequestDto request);
        Task<List<AiPracticeQuestionDto>> GeneratePracticeQuestionsAsync(AiPracticeQuestionsRequestDto request);
    }
}
