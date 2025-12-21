package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class WorkReport(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("userId")
    val userId: Int,
    
    @SerializedName("startTime")
    val startTime: String? = null,
    
    @SerializedName("finishTime")
    val finishTime: String? = null,
    
    @SerializedName("description")
    val description: String? = null
)

data class StartWorkRequest(
    @SerializedName("userId")
    val userId: Int,
    
    @SerializedName("startTime")
    val startTime: String? = null
)

data class FinishWorkRequest(
    @SerializedName("finishTime")
    val finishTime: String? = null
)

