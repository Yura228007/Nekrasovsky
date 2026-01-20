package com.nekrasovsky.android.ui.viewmodel

import android.app.Application
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.nekrasovsky.android.data.model.AlarmEvent
import com.nekrasovsky.android.data.repository.AppRepository
import com.nekrasovsky.android.data.signalr.SignalRService
import com.nekrasovsky.android.data.sound.AlarmSoundService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject

@HiltViewModel
class DashboardViewModel @Inject constructor(
    application: Application,
    private val repository: AppRepository,
    private val signalRService: SignalRService,
    private val alarmSoundService: AlarmSoundService
) : AndroidViewModel(application) {
    
    var currentUser = mutableStateOf<com.nekrasovsky.android.data.model.User?>(null)
        private set
    
    var alarmNotification = mutableStateOf<Triple<String, String, String>?>(null)
        private set

    init {
        // Подключаемся к SignalR при создании ViewModel
        viewModelScope.launch {
            try {
                signalRService.connect()
                
                // Настраиваем обработчик уведомлений
                signalRService.setOnAlarmNotification { message, location, user ->
                    // Воспроизводим звук
                    alarmSoundService.playAlarmSound()
                    
                    // Сохраняем уведомление для отображения в UI
                    alarmNotification.value = Triple(message, location, user)
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    fun setCurrentUser(user: com.nekrasovsky.android.data.model.User?) {
        currentUser.value = user
    }

    suspend fun createAlarmEvent(location: String, message: String?): Boolean {
        val user = currentUser.value
        if (user == null) {
            return false
        }

        return try {
            val alarmEvent = AlarmEvent(
                userId = user.id,
                location = location,
                message = message ?: "",
                createdAt = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.US).format(Date())
            )
            
            val response = repository.addAlarmEvent(alarmEvent)
            response.isSuccessful
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }

    override fun onCleared() {
        super.onCleared()
        viewModelScope.launch {
            signalRService.disconnect()
        }
    }
}
