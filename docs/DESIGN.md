# 4 in a Row — design breakdown

## Intent and contribution

Present a familiar strategy game with readable turns, immediate board feedback and a quick replay loop. Dima Rychka reports responsibility for game design, gameplay logic, all graphics and UI/UX in the presented version. The original Dataloma foundation is credited separately.

## Decisions visible in the source

| Decision | Player-facing effect | Evidence |
| --- | --- | --- |
| Choose computer or local two-player mode | Supports solo review and shared-device play | Local-mode selection in `GameManager` / `AiGameHud` |
| Show turn and thinking prompts | Makes it clear when input is expected | HUD status updates |
| Animate a piece to an exact cell | Makes the chosen column and final position visible | `DropLocalPiece` |
| Block input while a move resolves | Keeps turn transitions consistent | `IsBusy`, turn gating |
| Detect wins in four directions and handle full boards | Provides explicit outcomes | `ConnectFourRules` |
| Offer restart after play | Supports another attempt | `RestartGame` |

These effects are source-based design interpretations, not measured user-test results.

## What this adds to a playable-design portfolio

This project shows a discrete decision loop: understand the board, choose a column, read the consequence, then plan a response. It complements the continuous dragging in Glitter Peel! and the staged reward progression in Storage Auction 3.

## Next validation

- Test whether a first-time player can identify whose turn it is without explanation.
- Review computer response time and difficulty on the target device.
- Capture a short sequence showing mode selection, a legal move, a win and restart.
- Document the specific changes against the upstream repository if a detailed code-contribution comparison is needed.

The original foundation is [Dataloma/4InARow](https://github.com/Dataloma/4InARow), distributed with the retained MIT License. An upstream diff was not performed as part of packaging.
