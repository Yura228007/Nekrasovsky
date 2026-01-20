package com.nekrasovsky.android.data.sound

import android.content.Context
import android.media.AudioManager
import android.media.MediaPlayer
import android.media.ToneGenerator
import android.util.Log

class AlarmSoundService(private val context: Context) {
    private var mediaPlayer: MediaPlayer? = null
    private val toneGenerator = ToneGenerator(AudioManager.STREAM_NOTIFICATION, 100)

    fun playAlarmSound() {
        try {
            // Пытаемся использовать системный звук тревоги
            val audioManager = context.getSystemService(Context.AUDIO_SERVICE) as AudioManager
            
            // Используем ToneGenerator для воспроизведения тонального сигнала тревоги
            toneGenerator.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD, 2000)
            
            Log.d("AlarmSoundService", "Звук тревоги воспроизведен")
        } catch (e: Exception) {
            Log.e("AlarmSoundService", "Ошибка воспроизведения звука", e)
        }
    }

    fun stopAlarmSound() {
        try {
            toneGenerator.stopTone()
            mediaPlayer?.release()
            mediaPlayer = null
        } catch (e: Exception) {
            Log.e("AlarmSoundService", "Ошибка остановки звука", e)
        }
    }
}
