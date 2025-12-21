package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class AlarmEvent(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("userId")
    val userId: Int,
    
    @SerializedName("eventDate")
    val eventDate: String,
    
    @SerializedName("location")
    val location: String,
    
    @SerializedName("description")
    val description: String,
    
    @SerializedName("severity")
    val severity: String? = null
)

