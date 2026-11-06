# 4 in a Row
### Turn-based board-game prototype · Unity · Game Design

A 7 × 6 connect-four board with local two-player play, a computer opponent, animated piece drops and turn/result feedback.

## My contribution to this version

Game design, gameplay logic, all graphics and UI/UX.

**Original foundation:** [Dataloma/4InARow](https://github.com/Dataloma/4InARow). The source includes the original MIT License, copyright 2022 Dataloma, preserved in [LICENSE](LICENSE). The contribution statement above refers to the presented version and does not replace the original attribution.

## Open and play

- **Editor:** Unity **6000.6.3f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
- Add this repository folder in Unity Hub and allow package import to complete.
- Open `Assets/Scenes/Game.unity` and press Play.
- Choose the computer opponent or local two-player mode.
- Click or tap a column to drop a piece. Connect four horizontally, vertically or diagonally.
- Use the restart control to begin another match.
- Run `git lfs pull` after cloning to retrieve the artwork tracked with Git LFS.

The project also retains lobby and online-service code from its foundation. Online functionality is outside the verified scope of this portfolio package and may require Unity Services configuration.

## Design and source

[Design breakdown](docs/DESIGN.md)

| File | Purpose |
| --- | --- |
| `Assets/Scripts/ConnectFourRules.cs` | Legal moves, win detection and computer search |
| `Assets/Scripts/GameManager.cs` | Mode selection, turns, piece drops, results and restart |
| `Assets/Scripts/AiGameHud.cs` | Local mode selection and game-state prompts |
| `Assets/Scripts/BoardInput.cs` | Board interaction |
| `Assets/Editor/WebReleaseSettings.cs` | Existing WebGL export settings helper |

## Current status

The local source implements a computer search with alpha-beta pruning and a default depth of five. This is an implementation detail rather than a measured difficulty rating. Source-copy integrity was checked during packaging; gameplay, online services and browser performance were not rerun. A current gameplay capture and hosted demo are not included.

## Credits

See [CREDITS.md](CREDITS.md). Existing font notices and the original root license remain included.
