import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IngestFormComponent } from './components/ingest-form/ingest-form.component';
import { PriorityInboxComponent } from './components/priority-inbox/priority-inbox.component';

const routes: Routes = [
  { path: 'ingest', component: IngestFormComponent },
  { path: 'priority', component: PriorityInboxComponent },
  { path: '', redirectTo: 'ingest', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
