package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class Product(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("name")
    val name: String,
    
    @SerializedName("description")
    val description: String? = null,
    
    @SerializedName("code")
    val code: String? = null,
    
    @SerializedName("measuringUnit")
    val measuringUnit: String = "??"
)

