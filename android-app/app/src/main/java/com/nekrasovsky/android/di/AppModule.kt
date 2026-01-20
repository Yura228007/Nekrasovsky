package com.nekrasovsky.android.di

import android.content.Context
import com.nekrasovsky.android.data.signalr.SignalRService
import com.nekrasovsky.android.data.sound.AlarmSoundService
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object AppModule {
    
    @Provides
    @Singleton
    fun provideSignalRService(): SignalRService {
        return SignalRService()
    }
    
    @Provides
    @Singleton
    fun provideAlarmSoundService(@ApplicationContext context: Context): AlarmSoundService {
        return AlarmSoundService(context)
    }
}
