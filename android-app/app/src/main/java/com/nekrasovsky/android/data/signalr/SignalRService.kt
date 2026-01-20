package com.nekrasovsky.android.data.signalr

import android.util.Log
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import org.json.JSONObject

class SignalRService {
    private var hubConnection: HubConnection? = null
    private val baseUrl = "http://192.168.1.121:9000/" // TODO: Вынести в конфигурацию
    private var onAlarmNotification: ((String, String, String) -> Unit)? = null

    fun setOnAlarmNotification(callback: (message: String, location: String, user: String) -> Unit) {
        this.onAlarmNotification = callback
    }

    suspend fun connect() {
        try {
            if (hubConnection?.connectionState == com.microsoft.signalr.HubConnectionState.CONNECTED) {
                return
            }

            hubConnection = HubConnectionBuilder.create("${baseUrl}hubs/notifications")
                .withAutomaticReconnect()
                .build()

            // Обработка уведомлений о тревогах
            hubConnection?.on("AlarmNotification") { notification ->
                try {
                    val json = JSONObject(notification.toString())
                    val message = json.optString("message", "СОБЫТИЕ ТРЕВОГИ!")
                    val location = json.optString("location", "Неизвестно")
                    val user = json.optString("user", "Неизвестный пользователь")
                    val sound = json.optBoolean("sound", true)

                    onAlarmNotification?.invoke(message, location, user)
                } catch (e: Exception) {
                    Log.e("SignalRService", "Ошибка обработки уведомления", e)
                }
            }

            hubConnection?.start()
            Log.d("SignalRService", "SignalR подключен")

            // Подписываемся на уведомления о тревогах
            subscribeToAlarmNotifications()
        } catch (e: Exception) {
            Log.e("SignalRService", "Ошибка подключения к SignalR", e)
        }
    }

    suspend fun disconnect() {
        try {
            hubConnection?.stop()
            hubConnection = null
            Log.d("SignalRService", "SignalR отключен")
        } catch (e: Exception) {
            Log.e("SignalRService", "Ошибка отключения от SignalR", e)
        }
    }

    private suspend fun subscribeToAlarmNotifications() {
        try {
            hubConnection?.invoke("SubscribeToAlarmNotifications")
            Log.d("SignalRService", "Подписка на уведомления о тревогах активирована")
        } catch (e: Exception) {
            Log.e("SignalRService", "Ошибка подписки на уведомления", e)
        }
    }

    fun isConnected(): Boolean {
        return hubConnection?.connectionState == com.microsoft.signalr.HubConnectionState.CONNECTED
    }
}
