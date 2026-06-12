using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Congnex.Infrastructure.Services;

/// <summary>
/// Semantic Kernel plugin that gives Xyla a structured way to submit the
/// student's profile at the end of the interview. Using a KernelFunction
/// replaces the fragile &lt;xyla_plan&gt; JSON extraction with native tool-calling.
/// </summary>
public sealed class XylaInterviewPlugin
{
    private InterviewPlanData? _plan;

    [KernelFunction("complete_plan")]
    [Description(
        "Submit the student's profile after collecting all 6 interview answers. " +
        "Call this function EXACTLY ONCE when you are ready to generate the personalized study plan. " +
        "After calling it, deliver the final message to the student.")]
    public string CompletePlan(
        [Description("Student's CEFR English level inferred from the interview: A1, A2, B1, B2, C1, or C2")]
        string cefrLevel,

        [Description("Student's main learning goal from Pergunta 1: trabalho, viagem, estudos, or conexoes")]
        string studentGoal,

        [Description("Student's age in years (0 if not known)")]
        int age,

        [Description("Student's confidence with English: low, medium, or high")]
        string confidenceScore,

        [Description("Student's preferred learning style inferred from the conversation: visual, auditory, or reading")]
        string preferredLearningStyle,

        [Description("Short description of the ideal YouTube video topic in English, e.g. 'English for Work'")]
        string videoTopic,

        [Description("YouTube search query optimised for this student's CEFR level and goal, " +
                     "e.g. 'english conversation practice B1 intermediate work professional'")]
        string videoQuery)
    {
        _plan = new InterviewPlanData(
            CefrLevel:               cefrLevel,
            StudentGoal:             studentGoal,
            Age:                     age > 0 ? age : null,
            ConfidenceScore:         confidenceScore,
            PreferredLearningStyle:  preferredLearningStyle,
            VideoTopic:              videoTopic,
            VideoQuery:              videoQuery);

        return "Plan registered successfully. Now deliver the final message to the student.";
    }

    public bool         WasCalled => _plan is not null;
    public InterviewPlanData? Plan => _plan;
}

public record InterviewPlanData(
    string  CefrLevel,
    string  StudentGoal,
    int?    Age,
    string  ConfidenceScore,
    string  PreferredLearningStyle,
    string  VideoTopic,
    string  VideoQuery);
