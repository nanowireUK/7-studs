import {
    LogLevel,
    HttpTransportType,
    HubConnectionBuilder,
    JsonHubProtocol,
} from '@microsoft/signalr';

import {
    updateGame,
} from '../slices/game';

import {
    serverConnected,
    disconnected,
    reconnecting,
    awaitingResponse,
    setRejoinCode,
    setRoomId,
    setLeaverCount,
} from '../slices/hub';

import {
    serverUrl
} from '../../config';

export const connection = new HubConnectionBuilder()
    .configureLogging(LogLevel.Debug)
    .withUrl(`${serverUrl}/chatHub`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
    })
    .withHubProtocol(new JsonHubProtocol())
    .withAutomaticReconnect()
    .build();

export default (store) => {
    connection.on('ReceiveMyGameState', (msg) => {
        const game = JSON.parse(msg);
        const { MyRejoinCode, CountOfLeavers, RoomId } = game;

        localStorage.setItem('rejoinCode', MyRejoinCode);

        store.dispatch(setRejoinCode(MyRejoinCode));
        store.dispatch(setRoomId(RoomId));
        store.dispatch(setLeaverCount(CountOfLeavers));
        store.dispatch(updateGame(game));
        store.dispatch(awaitingResponse(false));
    });

    connection.on('ReceiveLeavingConfirmation', (msg) => {
        console.log('Received leave confirmation');
        console.log(msg);

        localStorage.setItem('rejoinCode', '');

        store.dispatch(setRejoinCode(''));
        store.dispatch(updateGame(null));
        connection.stop();
        window.location.reload();
    });

    connection
        .start()
        .then(() => store.dispatch(serverConnected()))
        .catch(() => store.dispatch(disconnected()));

    connection.onreconnected(() => store.dispatch(serverConnected()));
    connection.onreconnecting(() => store.dispatch(reconnecting()));
    connection.onclose(() => store.dispatch(disconnected()));

    return (next) => (action) => next(action);
};
