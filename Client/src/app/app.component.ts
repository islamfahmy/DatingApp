import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/User';
import { AccountService } from './_service/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'frontend';
  constructor(private accountService: AccountService) { }
  users: any;
  ngOnInit() {
    this.setCurrentUser()
  }
  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user') || '{}').token ? JSON.parse(localStorage.getItem('user') || '{}') : null;
    console.log(user)
    this.accountService.setCurrentUser(user)
  }

  

}
