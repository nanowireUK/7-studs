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
    setRejoinCode, } from '../slices/hub';

export const connection = new HubConnectionBuilder()
    .configureLogging(LogLevel.Debug)
    .withUrl('https://localhost:5001/chatHub', {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
    })
    .withHubProtocol(new JsonHubProtocol())
    .withAutomaticReconnect()
    .build();

export default (store) => {
    connection.on('ReceiveMyGameState', (msg) => {
        const { MyRejoinCode: rejoinCode, ...game } = JSON.parse(msg);

        console.log(rejoinCode);

        localStorage.setItem('rejoinCode', rejoinCode);

        store.dispatch(setRejoinCode(rejoinCode));
        store.dispatch(updateGame(game));
        store.dispatch(awaitingResponse(false));
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
