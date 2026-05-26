# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview
This Repository is used for a project to submit in India Innovates Hackathon 2026 by Municiple Coopration of Delhi the project aims to build Designing and building a platform that enables individuals, leaders, educators, institutions, and organizations to create realistic, interactive digital avatars capable of speaking, presenting, and engaging audiences in real time across multiple languages for governance, education, outreach, and large-scale communication. We are using Unity to add multiplayer for multiple platforms laptop/mobile VR where we make virtual avatars for everybody and they can walk around a spawn areas from their spawn area and participate in public/private meetings. their are 2 roles you can select viewer/creator during signup. The home page is made in react and then connects with unity and backend made in nodejs we are planning to add multiplayer in unity using relay we need to sync multiplayer and add all avatars for each player in unity so evveryone can view each other in multiplayer with respective avatar. we then need to setup mics and sound for everyone and allow viewers to give speech and let all viewers hear it if they are in that room. viewers can talk with each other using proximity chat but creators when they speak in a room they are heard by everyone. everyone is proximity chat only in spawn area. I want to make a rag bot for the creator and viewers the creator personal bot is a rag that summarises viewer chats and questions. and viewers have a common assistant bot which helps answer common questions 

## Key Components

### Home Page and react singup/signin and node backend
- unity web build is hosted on cloudflare currently
-there is a node backend with signup/sign in APIS and some other APIS we don't need to focus on rn
-the home page is made in react in meetverse-client folder there is basic signup/signin where we can select roles
-we need to create an unique theme for the whole project and implement that in website and app 


### Avatar Customisation and Multiplayer

Theres an initial customiser for everyone which is also the first scene of unity where we select our avatar and customise it that scene is done already 
Once they have selected their avatar the data is stored in node, the avatar is saved they can change it anytime they want.
remember the avatar customising is in unity but dashboard is in react I am gonna add node backend in this project
after we click complete I want users to load into a dashboard made in react where they can see all the rooms and can join them it should be an aesthetic well designed platform with viewers and organisers having a simillar dashbaord.


### Mic input output
Each user should be able to use mic in spawn area and rooms.
when in spawn area everyone has proximity chat but when in normal room made by an organiser when the organiser speaks each viewer can hear it from any area. also there will be seats and auditorium assests where viewers can sit and listen too
Viewers will have access to a normal chat too which they can use to talk to the organiser ask questions/participate in polls

### AI chatbot Rag 
There will be a rag chatbot built from langchain to accomodate viewer's questions and help the organiser see what viewers are saying and can use to prepare scripts etc

### multilingual support
-theres a node api for stt we will use that to create text from organiser speech and then convert to required language and show it as subtitles

### Current Architecture & React-Unity Bridge Flow (Already Implemented)
- **App Routing (React)**: The state flows through `landing` -> `customizer` -> `dashboard` -> `multiplayer`.
- **Unity Optimization**: To avoid slow WebGL reloading, `UnityEmbed.jsx` stays mounted in the DOM across UI states. 
- **Avatar Setup to Dashboard Flow**:
  1. User customises their avatar in Unity (Customizer scene).
  2. Clicking "Complete" triggers `SceneLoader.SaveAndLoadScene()`, which saves the avatar choices to `AvatarDataStore` cache and fires a JSLib event `NotifyAvatarComplete` to React (`window.dispatchReactEvent`).
  3. React intercepts this event, saves the avatar JSON data in state, and transitions to the `dashboard` UI.
  4. React uses CSS to smoothly shrink and reposition the WebGL Canvas into a 120x120px circular overlay on the sidebar.
  5. Concurrently, React calls `sendMessage("SceneLoader", "LoadDashboardScene")`. Unity loads `DashboardAvatarScene`, and a script called `DashboardAvatarManager.cs` automatically reads `AvatarDataStore` and applies the custom clothes to the avatar, resulting in a seamless 3D UI widget.
- **Frontend Theme**: Strict dark navy (`#131B2E`) and purple (`#9D75CB`) formal styling across all React components.

### Things claude should follow 
-- check existing code first before creating new code 
-- when starting to work on a new feature think deeply make implementation plan before executing
-- ultrathink before executing properly
-- make sure the code you're creating can be also implemented with future plans I have for the porject as mentioned in claude.md
-- keep code comments minimal but should explain the code in brief