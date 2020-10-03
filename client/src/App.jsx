import React from 'react';
import { useSelector } from 'react-redux';

import { selectGame } from './redux/slices/game';
import { selectConnectionState, ConnectionState } from './redux/slices/hub';

import Game from './Game';
import Welcome from './Welcome';
import Disconnected from './Disconnected';

function App() {
    const game = useSelector(selectGame);
    const connectionState = useSelector(selectConnectionState);

    if (connectionState === ConnectionState.DISCONNECTED) return <Disconnected />;
    if (game === null) return <Welcome />;
    return <Game />;
}

export default App;
