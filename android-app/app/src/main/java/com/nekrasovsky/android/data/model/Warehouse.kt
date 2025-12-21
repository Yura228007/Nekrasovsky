package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class Warehouse(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("name")
    val name: String,
    
    @SerializedName("type")
    val type: String,
    
    @SerializedName("location")
    val location: String? = null,
    
    @SerializedName("description")
    val description: String? = null
)

