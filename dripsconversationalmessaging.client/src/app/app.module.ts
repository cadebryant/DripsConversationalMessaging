import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { IngestFormComponent } from './components/ingest-form/ingest-form.component';
import { PriorityInboxComponent } from './components/priority-inbox/priority-inbox.component';

@NgModule({
  declarations: [
    AppComponent,
    IngestFormComponent,
    PriorityInboxComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
