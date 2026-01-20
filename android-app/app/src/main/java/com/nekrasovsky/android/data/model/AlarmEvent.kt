package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class AlarmEvent(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("userId")
    val userId: Int,
    
    @SerializedName("createdAt")
    val createdAt: String,
    
    @SerializedName("location")
    val location: String,
    
    @SerializedName("message")
    val message: String? = null
)

