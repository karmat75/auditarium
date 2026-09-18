## Model and reasoning policy

Work with the currently selected model and reasoning level whenever it is
sufficient. Optimize for low usage without sacrificing correctness.

Use the following classification:

- Mechanical, repetitive, or precisely scoped work:
  recommend GPT-5.6 Luna with Light or Medium reasoning.
- Normal implementation across a few related files:
  recommend GPT-5.6 Terra with Medium reasoning.
- Complex debugging, migrations, security-sensitive behavior, RBAC,
  authentication, or changes spanning multiple architectural layers:
  recommend GPT-5.6 Terra High or GPT-5.6 Sol High.
- Fundamental architectural decisions with significant long-term impact:
  recommend GPT-5.6 Sol Extra High.
- Recommend Max or Ultra only when the task genuinely requires it.

If the currently selected model or reasoning level is clearly insufficient:

1. Do not begin a large or risky implementation.
2. Explain briefly why escalation is appropriate.
3. Recommend the exact model and reasoning level.
4. Wait for the user to switch it and confirm continuation.

Do not recommend a more expensive model merely because it may produce a
slightly better result. Escalate only when the task's ambiguity, risk,
scope, or required judgment justifies it.