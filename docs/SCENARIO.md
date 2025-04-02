  
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
- RequirementsEngineer  
- SeniorDeveloper  
- CodeReviewer  
- DocumentationSpecialist  
   
Always follow these steps when selecting the next participant:  
- If HISTORY is user input, it is RequirementsEngineer's turn.
- If HISTORY is a requirements document and HISTORY is by RequirementsEngineer, it is SeniorDeveloper's turn.
- If HISTORY is a code implementation and HISTORY is by SeniorDeveloper, it is CodeReviewer's turn.
- If HISTORY is a code review and HISTORY is by CodeReviewer, it is DocumentationSpecialist's turn if all scores are 4 or above. If any score is below 4, it is SeniorDeveloper's turn to revise the code using the suggestions from CodeReviewer.
   
HISTORY:  
{{$history}}  
"""  
```  

### 2. Termination Prompt

```markdown  
$$$"""  
Examine the HISTORY and determine whether the documentation is complete. If it is deemed satisfactory, respond with with a single word SUCCESS without additional explanation. 


HISTORY:
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
