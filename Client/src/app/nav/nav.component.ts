import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { User } from '../_models/User';
import { AccountService } from '../_service/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) {

  }

  ngOnInit(): void {

  }
  login() {
    this.accountService.login(this.model).subscribe(res => {
      console.log(res);
      this.router.navigateByUrl('/members')
    });
  }
  logout() {
    this.accountService.logut();
    this.router.navigateByUrl('/')
  }

}