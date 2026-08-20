# BlogApp — Project Knowledge Transfer

## Overview
A full-stack blogging platform built with Blazor Server, MudBlazor, MongoDB, and Supabase authentication. Developed as an internship project.

---

## Tech Stack
| Technology | Purpose |
|---|---|
| C# / Blazor Server | Full-stack framework, runs on server via SignalR |
| MudBlazor | UI component library (Material Design) |
| MongoDB Atlas | Primary database (blog posts, user data) |
| Supabase | Authentication only (login, signup, JWT) |
| Markdig | Markdown to HTML conversion |
| Git + GitHub | Version control |
| Render | Planned deployment |
| Docker | Planned containerization |

---

## Project Structure
```
BlogApp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor         # App shell (AppBar, Drawer, session restore)
│   │   ├── MainLayout.razor.css
│   │   ├── ReconnectModal.razor     # Custom styled Blazor Server reconnect screen
│   │   └── ReconnectModal.razor.js
│   └── Shared/
│       └── LoadingLogo.razor        # Animated loading spinner component
├── Data/
│   ├── BlogRepository.cs            # CRUD for blog posts + visibility-filtered feed
│   ├── ConnectionRepository.cs      # Follow/friend relationships
│   ├── EngagementRepository.cs      # Likes, comments, saved posts
│   └── UserProfileRepository.cs    # User profiles, username lookup
├── Hubs/
│   └── NotificationHub.cs           # SignalR hub for real-time notifications
├── Models/
│   ├── BlogPost.cs                  # Blog post schema
│   ├── Comment.cs                   # Comment (postId, userId, content, createdAt)
│   ├── Like.cs                      # Like (postId, userId, createdAt)
│   ├── Notification.cs              # Notification schema
│   ├── SavedPost.cs                 # Saved post (postId, userId, createdAt)
│   ├── UserConnection.cs            # Follow/friend relationship schema
│   └── UserProfile.cs              # User profile schema (username, bio, etc.)
├── Pages/
│   ├── Home.razor                   # Main feed (Instagram-style vertical cards)
│   ├── BlogEditor.razor             # Markdown editor (create + edit posts)
│   ├── PostView.razor               # Full post reader with likes, comments, save
│   ├── UserProfile.razor            # Profile: posts, social counts, dialog lists
│   ├── ProfileSettings.razor        # Edit username, bio, display name
│   ├── Notifications.razor          # Notification list with mark-all-read
│   ├── Login.razor                  # Supabase login
│   ├── Signup.razor                 # Supabase signup + username creation
│   └── NotFound.razor               # 404 page
├── Services/
│   ├── DatabaseService.cs           # Singleton MongoDB connection
│   ├── NotificationService.cs       # Save + push notifications via SignalR
│   └── UserService.cs              # Scoped session state + OnSessionReady event
├── wwwroot/                         # Static assets + custom CSS
├── appsettings.json
├── appsettings.Development.json     # Dev secrets (gitignored)
└── Program.cs                       # DI registration + pipeline
```

---

## Requirements

### Completed

**Foundation**
- Project scaffolding, Git + GitHub, MudBlazor layout
- Supabase authentication (Login, Signup) + route protection
- MongoDB Atlas connected via DatabaseService + BlogRepository
- BlogPost model with full schema
- Hard refresh session recovery (ProtectedLocalStorage — access_token + refresh_token)
- UserService.OnSessionReady event — pages re-render after session restores

**Blog Features**
- Blog editor with real-time markdown preview (Markdig)
- Posts: create, edit (/editor/{postId}), delete (inline confirmation)
- Post visibility change from the live post view (dropdown)
- Tag chips, post slug, summary

**Home Feed**
- Instagram-style vertical single-column card layout
- Visibility-filtered feed (Public / Followers / Private rules)
- Inline Follow button on cards for authors you don't follow
- Like/Unlike: filled vs outline heart icon + count (batch loaded)
- Save/Unsave: bookmark icon on each card
- Author @username, avatar, date on each card header
- Tag chips on each card
- "Nothing to read yet" empty state with Write button

**PostView**
- Markdown rendered to HTML (Markdig)
- Author avatar + @username clickable to profile
- Visibility badge pill
- Like/Unlike with real-time count
- Save/Unsave bookmark
- Comments: add comment, full threaded list with author avatars + timestamps
- Author controls: Edit, Delete (with confirmation), Visibility change dropdown

**User Profiles**
- /profile/{userId}: avatar, @username, post count, follower/following/friend counts
- Clickable counts: Instagram-style overlay list dialog showing users
- Follow/Unfollow, Add Friend/Cancel/Accept (context-aware)
- Pending friend requests section (own profile only)
- Post grid filtered by viewer's relationship (Public/Followers/Private)
- Own profile: "Your Profile" chip shown, no action buttons
- OnParametersSetAsync: profile reloads when navigating between profiles

**Username System**
- UserProfile model in MongoDB
- Unique username validation (lowercase, numbers, underscores)
- Set at signup, editable in Settings (/settings)
- @username shown across: feed cards, PostView, profile, notifications, dialog lists

**Social / Connection System**
- Follow/Unfollow (instant), Friend requests (pending -> accepted)
- Self-follow and self-friend-request blocked
- Follower/Following/Friends query methods

**Engagement System**
- Models: Like, Comment, SavedPost (all in MongoDB)
- EngagementRepository:
  - ToggleLikeAsync (like/unlike, returns new bool state)
  - GetLikeCountsAsync (batch — one DB call for all posts on feed)
  - GetLikedPostIdsAsync (which posts the viewer has liked)
  - AddCommentAsync, GetCommentsAsync, GetCommentCountsAsync
  - ToggleSaveAsync, GetSavedPostIdsAsync, GetAllSavedPostIdsAsync

**Notifications**
- Real-time bell badge (SignalR + MudBadge)
- Snackbar toast on incoming notification
- Notifications page: list, unread highlight, timestamps in local time
- Mark all as read
- Sent on: follow, friend request, request accepted
- Bell resets on navigate to /notifications

**Auth & Layout**
- Sidebar nav hidden when not logged in
- Logout: signs out, clears storage, redirects to /login
- ReconnectModal: custom styled reconnect screen
- LoadingLogo: reusable animated loading component

**Bug Fixes**
- Own profile showing Follow buttons (OnSessionReady timing fix)
- Followers dialog not opening (span @onclick -> MudButton OnClick)
- stopPropagation on MudPaper ignored -> moved to plain div wrapper
- Profile showing all posts regardless of visibility -> filtered by relationship
- Stale profile data on navigation -> OnParametersSetAsync
- Self-follow/self-friend blocked
- Avatar showing "@" in dialog -> TrimStart('@')
- Duplicate follow inserts -> guard in FollowAsync
- IsFollowingAsync checks "follow" type only (not "friend")

### Pending
- RSS feeds (Public + Private)
- Blog post AI summary (OpenAI API)
- Docker containerisation
- Deployment on Render

---

## Key Files & Their Roles

### UserService.cs
- Per-circuit state: UserId, Email, AccessToken, RefreshToken, IsLoggedIn
- OnSessionReady event: fired by MainLayout after session restore

### MainLayout.razor
- App shell + route protection
- Restores Supabase session from ProtectedLocalStorage on hard refresh
- Fires UserService.NotifySessionReady() after restore
- Manages notification bell (SignalR subscription + count)

### EngagementRepository.cs
- Likes, comments, saved posts CRUD
- Batch queries for feed performance (no N+1 queries)

### ConnectionRepository.cs
- Self-interaction guards (self-follow, self-friend blocked)
- Duplicate follow guard
- Follow/friend state queries

### UserProfile.razor
- OnParametersSetAsync: detects URL change and reloads for new profile
- Subscribes to OnSessionReady for correct own-profile detection
- Post visibility filtered by viewer relationship

---

## Architecture Decisions
- Blazor Server: SignalR built-in for real-time
- Supabase: auth only (JWT, sessions)
- MongoDB: flexible NoSQL for blog content
- Scoped UserService: per-user session state
- Singleton DatabaseService: shared DB connection
- Repository pattern: DB logic separated from pages
- Batch engagement queries: one DB call per feed load

---

## Environment Setup
1. Clone repo
2. Create appsettings.Development.json:
```json
{
  "Supabase": { "Url": "...", "AnonKey": "..." },
  "MongoDB": { "ConnectionString": "...", "DatabaseName": "BlogApp" }
}
```
3. dotnet restore
4. dotnet run
5. Stop: Ctrl+C in terminal

---

## Daily Work Log
| Date | Work Done |
|---|---|
| 5th June | Project setup, Git, MudBlazor layout |
| 8th June | Supabase auth, Login/Signup, route protection |
| 9th June | MongoDB, BlogPost model, BlogRepository, editor with markdown preview |
| 15th June | Home feed (MudGrid cards), PostView (Markdown, author info, tags) |
| 16th June | Fixed crashes. UserConnection model, ConnectionRepository |
| 17th June | UserProfile page, GetByAuthorIdAsync, clickable author names |
| 18th June | GetFeedAsync visibility rules, visibility badges, logout, sidebar closed by default |
| 20th June | Notification bell, Notifications page, snackbar, duplicate follow fix |
| 22nd June | isFollowing fix, bell reset fix, improved post cards, timestamp fix, sidebar nav hidden |
| 23rd June | Username system, post edit/delete/visibility change |
| 29th June | Followers dialog fix (MudButton), own-profile button fix (OnSessionReady), stopPropagation fix |
| 30th June | Profile navigation fix (OnParametersSetAsync), post visibility filtering on profile, self-follow guard, avatar fix |
| July | Engagement system (Like/Comment/SavedPost), PostView comments + like + save, Home feed redesign (Instagram-style), LoadingLogo component, Notifications page improvements |

---

## Known Issues / Tech Debt
| Issue | Cause | Fix Plan |
|---|---|---|
| Comment author names not batch loaded | Each comment fetches profile separately | Add GetDisplayNamesAsync batch call when loading comments |
