import React from 'react';
import { useSelector } from 'react-redux';

import { selectGame, selectInLobby } from './redux/slices/game';
import { selectConnectionState, ConnectionState } from './redux/slices/hub';

import Game from './Game';
import Welcome from './Welcome';
import Disconnected from './Disconnected';
import Lobby from './Lobby';

function App() {
    const game = useSelector(selectGame);
    const connectionState = useSelector(selectConnectionState);
    const inLobby = useSelector(selectInLobby);

    if (connectionState === ConnectionState.DISCONNECTED) return <Disconnected />;
    if (game === null) return <Welcome />;
    if (inLobby) return <Lobby />
    return <Game />;
}

export default App;
