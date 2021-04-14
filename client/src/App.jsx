import React from 'react';
import { useSelector } from 'react-redux';

import { selectGame, selectInLobby } from './redux/slices/game';
import { selectConnectionState, ConnectionState } from './redux/slices/hub';
import { selectCurrentView, Views } from './redux/slices/views';

import Game from './views/Game';
import WelcomeView from './views/Welcome';
import Disconnected from './views/Disconnected';
import Lobby from './views/Lobby';
import CreateRoomView from './views/CreateRoom';
import JoinRoomView from './views/JoinRoom';

function App() {
    const game = useSelector(selectGame);
    const connectionState = useSelector(selectConnectionState);
    const inLobby = useSelector(selectInLobby);
    const currentView = useSelector(selectCurrentView);

    if (connectionState === ConnectionState.DISCONNECTED) return <Disconnected />;
    if (game === null) {
        switch (currentView) {
        case Views.WELCOME:
            return <WelcomeView />;
        case Views.CREATE_ROOM:
            return <CreateRoomView />;
        case Views.JOIN_ROOM:
            return <JoinRoomView />;
        default:
        }
    }
    if (inLobby) return <Lobby />;
    return <Game />;
}

export default App;
