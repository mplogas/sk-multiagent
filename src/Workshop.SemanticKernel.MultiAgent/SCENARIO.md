  
## Description  
   
This scenario demonstrates a collaborative feature development process using multiple specialized agents. The workflow guides a feature request from initial user input through requirements engineering, implementation, code review, documentation, and final deployment. The process ensures quality through iterative feedback and includes a termination step to conclude the workflow based on the documentation's completeness.  
   
## Agents Involved  
   
- **Requirements Engineer**  
  - **Role:** Transforms user input into detailed functional and non-functional requirements.  
  - **Action:** Creates a structured requirements document with acceptance criteria.  
   
- **Senior Developer**  
  - **Role:** Implements the feature based on the provided requirements.  
  - **Action:** Develops C# code, including unit tests, adhering to best practices and design patterns.  
  - **Additional Responsibility:** Revises code based on feedback from the Code Reviewer.  
   
- **Code Reviewer**  
  - **Role:** Evaluates the implemented code against the requirements and various quality criteria.  
  - **Action:** Provides an evaluation report with ratings (0-5) for Feature Completeness, Best Practices, Performance, and Security.  
  - **Additional Responsibility:** Offers suggestions for improvement if any scores are below 4.  
   
- **Documentation Specialist**  
  - **Role:** Prepares comprehensive documentation for the developed feature.  
  - **Action:** Creates API documentation, user manuals, and a changelog.  
  - **Termination Responsibility:** Provides a sign-off as SUCCESS or FAILURE based on the completeness of the documentation.  
   
- **Termination Agent**  
  - **Role:** Determines the final outcome of the process.  
  - **Action:** Responds with SUCCESS if documentation is complete or FAILURE if additional human interaction is required.  
   
## Prompts  
   
### 1. Agent Selection Function Prompt  
   
```markdown  
$$$"""  
Your job is to determine which participant takes the next turn in the feature development process based on the action of the most recent participant.  
State only the name of the participant to take the next turn.  
   
Choose only from these participants:  
- {{{Requirements Engineer}}}  
- {{{Senior Developer}}}  
- {{{Code Reviewer}}}  
- {{{Documentation Specialist}}}  
   
Always follow these steps when selecting the next participant:  
1) After user input, it is {{{Requirements Engineer}}}'s turn.  
2) After {{{Requirements Engineer}}} provides the requirements document, it's {{{Senior Developer}}}'s turn.  
3) After {{{Senior Developer}}} implements the feature, it's {{{Code Reviewer}}}'s turn.  
4) After {{{Code Reviewer}}} submits the evaluation:  
    a) If all scores are 4 or above, it's {{{Documentation Specialist}}}'s turn.  
    b) If any score is below 4, it's {{{Senior Developer}}}'s turn to revise the code. The Senior Developer must incorporate all suggestions provided by the Code Reviewer into the revised code.  
5) After {{{Senior Developer}}} revises the code, return to {{{Code Reviewer}}} for re-evaluation.  
6) Repeat steps 3 to 5 until all evaluation scores from the Code Reviewer are 4 or above.  
7) After {{{Documentation Specialist}}} prepares the documentation, the Documentation Specialist provides a sign-off as SUCCESS or FAILURE.  
    a) If SUCCESS, the process proceeds to Pull Request and Deployment.  
    b) If FAILURE, the process may require further revisions (specify next steps if necessary).  
   
History:  
{{$history}}  
"""  
```  

### 2. Termination Prompt

```markdown  
$$$"""  
Your job is to determine if the documentation is complete and no additional human interaction is required. If everything is fine, respond with a single word: SUCCESS. If additional interaction is needed, respond with: FAILURE.  
   
History:  
   
{{$history}}  
"""  
```  

## Termination Criteria

The process concludes with a sign-off from the Documentation Specialist:

- **SUCCESS:** Documentation is complete, and the feature is ready for Pull Request (PR) submission and deployment.
- **FAILURE:** Additional revisions or interactions are required before proceeding.

## Additional Parameters

- **Maximum Iterations:** To prevent infinite loops between the Senior Developer and Code Reviewer, set a maximum number of iterations (e.g., 5). If this limit is reached without achieving all scores of 4 or above, the process should escalate for human intervention.

## Workflow Overview

1. **User Input:** The user submits a feature request.
2. **Requirements Engineering:** The Requirements Engineer drafts a detailed requirements document.
3. **Feature Implementation:** The Senior Developer writes the C# code based on the requirements.
4. **Code Review:** The Code Reviewer assesses the code and provides ratings and suggestions.
5. **Iterative Improvement:** If necessary, the Senior Developer revises the code incorporating feedback. This loop continues until all ratings are satisfactory or the maximum number of iterations is reached.
6. **Documentation Preparation:** The Documentation Specialist creates the necessary documentation and signs off on its completeness.
7. **Pull Request and Deployment:** Upon SUCCESS, the feature can be integrated into the main codebase and deployed. If FAILURE, the process may require further revisions.
