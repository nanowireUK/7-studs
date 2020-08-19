import { LogLevel, HttpTransportType, HubConnectionBuilder, JsonHubProtocol } from '@microsoft/signalr';
import { updateGame, awaitingResponse, connected, disconnected, reconnecting } from '../slices/game';

export const connection = new HubConnectionBuilder()
    .configureLogging(LogLevel.Debug)
    .withUrl("https://localhost:5001/chatHub", {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
    })
    .withHubProtocol(new JsonHubProtocol())
    .withAutomaticReconnect()
    .build();

export default store => {
    connection.on('ReceiveUpdatedGameState', (msg) => {
        store.dispatch(updateGame(JSON.parse(msg)));
        store.dispatch(awaitingResponse(false));
    });

    connection.start().then(() => {
        store.dispatch(connected());
    }).catch(() => {
        store.dispatch(disconnected());
    })

    connection.onreconnected(() => {
        store.dispatch(connected());
    });

    connection.onreconnecting(() => {
        store.dispatch(reconnecting());
    });

    connection.onclose(() => {
        store.dispatch(disconnected());
    })

    return next => action => next(action);
}